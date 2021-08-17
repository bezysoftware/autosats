using AutoSats.Data;
using AutoSats.Exceptions;
using AutoSats.Execution.Services;
using AutoSats.Extensions;
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
        private readonly string dataFolder;

        public ExchangeScheduleRunner(
            SatsContext db,
            ILogger<ExchangeScheduleRunner> logger,
            IExchangeService exchangeService,
            IWalletService walletService)
        {
            this.db = db;
            this.logger = logger;
            this.exchangeService = exchangeService;
            this.walletService = walletService;

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

            this.exchangeService.Initialize(keys, schedule.Exchange);

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
                var (_, fiatCurrency) = Currency.Parse(schedule.CurrencyPair);
                var balance = await GetCurrencyBalance(fiatCurrency);
                if (balance < schedule.Spend)
                {
                    throw new ScheduleRunFailedException($"Cannot spend '{schedule.Spend}' of '{schedule.CurrencyPair}' because the balance is only '{balance}'");
                }

                var price = await this.exchangeService.GetPriceAsync(schedule.CurrencyPair);
                var amount = schedule.Spend / price;

                this.logger.LogInformation($"Going to buy '{amount}' of '{schedule.CurrencyPair}'");

                var result = await this.exchangeService.BuyAsync(schedule.CurrencyPair, amount);

                this.db.ExchangeBuys.Add(new ExchangeEventBuy
                {
                    Schedule = schedule,
                    Received = result.Amount,
                    Price = result.AveragePrice,
                    OrderId = result.OrderId,
                    Timestamp = DateTime.UtcNow
                });

                this.logger.LogInformation($"Bought '{result.Amount}' of '{schedule.CurrencyPair}' w/ avg price '{result.AveragePrice}', order id: '{result.OrderId}'");
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

        private async Task WithdrawAsync(ExchangeSchedule schedule)
        {
            if (schedule.WithdrawalType == ExchangeWithdrawalType.None)
            {
                return;
            }

            var (cryptoCurrency, _) = Currency.Parse(schedule.CurrencyPair);
            var balance = await GetCurrencyBalance(cryptoCurrency);
            if (balance < schedule.WithdrawalAmount)
            {
                this.logger.LogInformation($"{cryptoCurrency} balance {balance} is less than withdrawal amount {schedule.WithdrawalAmount}");
                return;
            }

            var address = schedule.WithdrawalType switch
            {
                ExchangeWithdrawalType.Fixed => schedule.WithdrawalAddress,
                ExchangeWithdrawalType.Dynamic => await this.walletService.GenerateDepositAddressAsync(),
                _ => throw new InvalidOperationException($"Unknown withdrawal type '{schedule.WithdrawalType}'")
            };

            this.logger.LogInformation($"Going to withdraw '{schedule.WithdrawalAmount}' of '{cryptoCurrency}' to address '{address}'");

            try
            {
                var id = await this.exchangeService.WithdrawAsync(cryptoCurrency, address, schedule.WithdrawalAmount);

                this.db.ExchangeWithdrawals.Add(new ExchangeEventWithdrawal
                {
                    Schedule = schedule,
                    Address = address,
                    Amount = schedule.WithdrawalAmount,
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

        private async Task<decimal> GetCurrencyBalance(string currency)
        {
            var balances = await this.exchangeService.GetBalancesAsync();

            if (balances.TryGetValue(currency.ToUpper(), out var balance) || balances.TryGetValue(currency.ToLower(), out balance))
            {
                return balance;
            }

            return 0;
        }

    }
}
