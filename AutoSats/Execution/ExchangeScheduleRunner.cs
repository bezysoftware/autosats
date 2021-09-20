using AutoSats.Configuration;
using AutoSats.Data;
using AutoSats.Exceptions;
using AutoSats.Execution.Services;
using AutoSats.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSats.Execution
{
    public class ExchangeScheduleRunner : IExchangeScheduleRunner
    {
        private readonly SatsContext db;
        private readonly ILogger<ExchangeScheduleRunner> logger;
        private readonly IExchangeService exchangeService;
        private readonly IWalletService walletService;
        private readonly ExchangeOptions[] exchangeOptions;
        private readonly string dataFolder;

        public ExchangeScheduleRunner(
            SatsContext db,
            ILogger<ExchangeScheduleRunner> logger,
            IExchangeService exchangeService,
            IWalletService walletService,
            ExchangeOptions[] exchangeOptions)
        {
            this.db = db;
            this.logger = logger;
            this.exchangeService = exchangeService;
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

            this.exchangeService.Initialize(schedule.Exchange, keys);

            try
            {
                await BuyAsync(schedule);
                await WithdrawAsync(schedule);
            }
            finally
            {
                await this.db.SaveChangesAsync();
            }
        }

        private async Task BuyAsync(ExchangeSchedule schedule)
        {
            try
            {
                var spendCurrency = schedule.SpendCurrency;
                var balance = await GetCurrencyBalance(spendCurrency);

                if (balance < schedule.Spend)
                {
                    throw new ScheduleRunFailedException($"Cannot spend '{schedule.Spend}' of '{schedule.Symbol}' because the balance is only '{balance}'");
                }

                var price = await this.exchangeService.GetPriceAsync(schedule.Symbol);
                var currenciesReversed = GetExchangeOptions(schedule.Exchange).ReverseCurrencies;
                var invert = !schedule.Symbol.EndsWith(spendCurrency, StringComparison.OrdinalIgnoreCase) ^ currenciesReversed;
                var amount = invert ? schedule.Spend : schedule.Spend / price;

                this.logger.LogInformation($"Going to buy '{amount}' of '{schedule.Symbol}'");

                var orderType = this.exchangeOptions.FirstOrDefault(x => x.Name == schedule.Exchange)?.BuyOrderType ?? BuyOrderType.Market;
                var result = await this.exchangeService.BuyAsync(schedule.Symbol, amount, orderType, invert);

                this.db.ExchangeBuys.Add(new ExchangeEventBuy
                {
                    Schedule = schedule,
                    Received = invert ? result.AveragePrice * result.Amount : result.Amount,
                    Price = result.AveragePrice,
                    OrderId = result.OrderId,
                    Timestamp = DateTime.UtcNow
                });

                this.logger.LogInformation($"Bought '{result.Amount}' of '{schedule.Symbol}' w/ avg price '{result.AveragePrice}', order id: '{result.OrderId}'");
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

        private async Task WithdrawAsync(ExchangeSchedule schedule)
        {
            if (schedule.WithdrawalType == ExchangeWithdrawalType.None)
            {
                return;
            }

            var withdrawCurrency = "BTC";
            var balance = await GetCurrencyBalance(withdrawCurrency);
            
            if (balance < schedule.WithdrawalLimit)
            {
                this.logger.LogInformation($"{withdrawCurrency} balance {balance} is less than withdrawal limit {schedule.WithdrawalLimit}");
                return;
            }
            
            var options = GetExchangeOptions(schedule.Exchange);
            var fee = await GetWithdrawalFee(withdrawCurrency, options.FallbackWithdrawalFee);

            var address = schedule.WithdrawalType switch
            {
                ExchangeWithdrawalType.Fixed => schedule.WithdrawalAddress ?? throw new InvalidOperationException("WithdrawalType is Fixed, but address is null"),
                ExchangeWithdrawalType.Dynamic => await this.walletService.GenerateDepositAddressAsync(),
                _ => throw new InvalidOperationException($"Unknown withdrawal type '{schedule.WithdrawalType}'")
            };

            this.logger.LogInformation($"Going to withdraw '{balance}' - '{fee}' fee reserve of '{withdrawCurrency}' to address '{address}'");

            try
            {
                var id = await this.exchangeService.WithdrawAsync(withdrawCurrency, address, balance - fee);

                this.db.ExchangeWithdrawals.Add(new ExchangeEventWithdrawal
                {
                    Schedule = schedule,
                    Address = address,
                    Amount = schedule.WithdrawalLimit,
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

        private async Task<decimal> GetWithdrawalFee(string cryptoCurrency, decimal fallbackFee)
        {
            try
            {
                var fee = await this.exchangeService.GetWithdrawalFeeAsync(cryptoCurrency);

                return fee == 0 
                    ? fallbackFee
                    : fee;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Couldn't get withdrawal fee for {cryptoCurrency}, using default");
                return fallbackFee;
            }
        }

        private async Task<decimal> GetCurrencyBalance(string currency)
        {
            var c = currency.ToUpper();
            var balances = await this.exchangeService.GetBalancesAsync();

            return balances.FirstOrDefault(x => x.Currency.ToUpper() == c)?.Amount ?? 0;
        }
    }
}
