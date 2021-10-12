using AutoSats.Configuration;
using AutoSats.Data;
using AutoSats.Exceptions;
using AutoSats.Execution.Services;
using AutoSats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSats.Execution
{
    public class ExchangeScheduleRunner : IExchangeScheduleRunner
    {
        private readonly SatsContext db;
        private readonly ILogger<ExchangeScheduleRunner> logger;
        private readonly IExchangeServiceFactory exchangeServiceFactory;
        private readonly IWalletService walletService;
        private readonly IEnumerable<ExchangeOptions> exchangeOptions;
        private readonly string dataFolder;

        public ExchangeScheduleRunner(
            SatsContext db,
            ILogger<ExchangeScheduleRunner> logger,
            IExchangeServiceFactory exchangeServiceFactory,
            IWalletService walletService,
            IEnumerable<ExchangeOptions> exchangeOptions)
        {
            this.db = db;
            this.logger = logger;
            this.exchangeServiceFactory = exchangeServiceFactory;
            this.walletService = walletService;
            this.exchangeOptions = exchangeOptions;

            // set data folder to the same location where the db is saved
            this.dataFolder = db.Database.GetDbConnection().ConnectionString
                .Split(";")
                .Select(x => x.Split("="))
                .Where(x => x.Length == 2 && x[0].Equals("Data Source", StringComparison.OrdinalIgnoreCase))
                .Select(x => Path.GetDirectoryName(x[1]))
                .FirstOrDefault() ?? "/app_data/";
        }

        public string KeysPath => this.dataFolder;

        public async Task RunScheduleAsync(int id)
        {
            var schedule = await this.db.ExchangeSchedules.FirstAsync(x => x.Id == id);
            var keys = Path.Combine(this.dataFolder, $"{id}.{ExecutionConsts.KeysFileExtension}");

            using var service = await this.exchangeServiceFactory.GetServiceAsync(schedule.Exchange, keys);

            try
            {
                await BuyAsync(service, schedule);
                await WithdrawAsync(service, schedule);
            }
            finally
            {
                await this.db.SaveChangesAsync();
            }
        }

        private async Task BuyAsync(IExchangeService service, ExchangeSchedule schedule)
        {
            try
            {
                var spendCurrency = schedule.SpendCurrency;
                var (_, balance) = await GetCurrencyBalance(service, spendCurrency);

                if (balance < schedule.Spend)
                {
                    throw new ScheduleRunFailedException($"Cannot spend '{schedule.Spend}' of '{spendCurrency}' because the balance is only '{balance}'");
                }

                var price = await service.GetPriceAsync(schedule.Symbol);
                var currenciesReversed = GetExchangeOptions(schedule.Exchange).ReverseCurrencies;
                var invert = !schedule.Symbol.EndsWith(spendCurrency, StringComparison.OrdinalIgnoreCase) ^ currenciesReversed;
                var amount = invert ? schedule.Spend : schedule.Spend / price;

                this.logger.LogInformation($"Going to buy '{amount}' of '{schedule.Symbol}'");

                var orderType = this.exchangeOptions.FirstOrDefault(x => x.Name == schedule.Exchange)?.BuyOrderType ?? BuyOrderType.Market;
                var result = await service.BuyAsync(schedule.Symbol, amount, orderType, invert);
                var buy = new ExchangeEventBuy
                {
                    Schedule = schedule,
                    Received = invert ? result.AveragePrice * result.Amount : result.Amount,
                    Price = result.AveragePrice,
                    OrderId = result.OrderId,
                    Timestamp = DateTime.UtcNow
                };

                this.db.ExchangeBuys.Add(buy);

                this.logger.LogInformation($"Received '{buy.Received}' of '{schedule.Symbol}' w/ avg price '{buy.Price}', order id: '{buy.OrderId}'");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Buy failed for schedule {schedule.Id}");
                this.db.ExchangeBuys.Add(new ExchangeEventBuy
                {
                    Schedule = schedule,
                    Timestamp = DateTime.UtcNow,
                    Error = ex.InnerException == null ? ex.Message : ex.ToString()
                });

                throw;
            }
        }

        private ExchangeOptions GetExchangeOptions(string exchange)
        {
            return this.exchangeOptions.FirstOrDefault(e => e.Name == exchange) ?? new ExchangeOptions();
        }

        private async Task WithdrawAsync(IExchangeService service, ExchangeSchedule schedule)
        {
            if (schedule.WithdrawalType == ExchangeWithdrawalType.None)
            {
                return;
            }

            var options = GetExchangeOptions(schedule.Exchange);
            var (withdrawCurrency, balance) = await GetCurrencyBalance(service, options.BitcoinSymbol);
            
            if (balance < schedule.WithdrawalLimit)
            {
                this.logger.LogInformation($"{withdrawCurrency} balance {balance} is less than withdrawal limit {schedule.WithdrawalLimit}");
                return;
            }

            var amount = balance - ExecutionConsts.FeeReserve;
            var address = schedule.WithdrawalType switch
            {
                ExchangeWithdrawalType.Fixed => schedule.WithdrawalAddress ?? throw new InvalidOperationException("WithdrawalType is Fixed, but address is null"),
                ExchangeWithdrawalType.Named => null,
                ExchangeWithdrawalType.Dynamic => await this.walletService.GenerateDepositAddressAsync(),
                _ => throw new InvalidOperationException($"Unknown withdrawal type '{schedule.WithdrawalType}'")
            };

            this.logger.LogInformation($"Going to withdraw '{amount}' of '{withdrawCurrency}' to address '{address}'");

            try
            {
                var id = await service.WithdrawAsync(withdrawCurrency, address, amount);

                this.db.ExchangeWithdrawals.Add(new ExchangeEventWithdrawal
                {
                    Schedule = schedule,
                    Address = address,
                    Amount = amount,
                    Timestamp = DateTime.UtcNow,
                    WithdrawalId = id
                });

                this.logger.LogInformation($"Withdrawal succeeded with id '{id}'");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Withdrawal failed for schedule {schedule.Id}");
                this.db.ExchangeWithdrawals.Add(new ExchangeEventWithdrawal
                {
                    Schedule = schedule,
                    Timestamp = DateTime.UtcNow,
                    Error = ex.InnerException == null ? ex.Message : ex.ToString()
                });

                throw;
            }
        }

        private async Task<(string currency, decimal balance)> GetCurrencyBalance(IExchangeService service, string currency, string fallbackCurrency = "BTC")
        {
            var c = currency.ToUpper();
            var balances = await service.GetBalancesAsync();

            // some exchanges use a different symbol for trading (XXBT) and for reporting balance (BTC), try both 
            var balanceCurrency = balances.FirstOrDefault(x => x.Currency.ToUpper() == c);

            if (balanceCurrency != null)
            {
                return (c, balanceCurrency.Amount);
            }

            var balanceFallback = balances.FirstOrDefault(x => x.Currency.ToUpper() == fallbackCurrency);

            if (balanceFallback != null)
            {
                return (c, balanceFallback.Amount);
            }

            return (currency, 0);
        }
    }
}
