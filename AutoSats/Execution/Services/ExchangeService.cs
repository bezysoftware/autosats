using AutoSats.Exceptions;
using AutoSats.Models;
using ExchangeSharp;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSats.Execution.Services
{
    public class ExchangeService : IExchangeService, IDisposable
    {
        private ExchangeAPI? api;
        private readonly ILogger<ExchangeService> logger;

        private ExchangeAPI Api => this.api ?? throw new InvalidOperationException("ExchangeService has not been initialized");

        public ExchangeService(ILogger<ExchangeService> logger)
        {
            this.logger = logger;
        }

        public void Initialize(string exchangeName, string? keysFileName)
        {
            // todo: no need to cast in new version https://github.com/jjxtra/ExchangeSharp/issues/621
            var api = (ExchangeAPI)ExchangeAPI.GetExchangeAPI(exchangeName);

            if (keysFileName != null)
            {
                api.LoadAPIKeys(keysFileName);
            }

            this.api = api;
        }

        public async Task<CheckConnectionResult> CheckConnectionAndInitializeAsync(string exchangeName, string key1, string key2, string? key3)
        {
            var api = (ExchangeAPI)ExchangeAPI.GetExchangeAPI(exchangeName);

            try
            {
                api.LoadAPIKeysUnsecure(key1, key2, key3);
                await api.GetAmountsAvailableToTradeAsync();
                this.api = api;
                return new CheckConnectionResult(true);
            }
            catch(Exception ex)
            {
                this.logger.LogError($"Check connection failed with supplied keys for exchange {exchangeName}", ex);
                return new CheckConnectionResult(false, ex.Message);
            }
        }

        public async Task<BuyResult> BuyAsync(string pair, decimal amount)
        {
            var result = await Api.PlaceOrderAsync(new ExchangeOrderRequest
            {
                Amount = amount,
                IsBuy = true,
                MarketSymbol = pair,
                OrderType = OrderType.Market
            });

            // query order details until it is fully filled
            while (result.Result == ExchangeAPIOrderResult.FilledPartially || result.Result == ExchangeAPIOrderResult.Pending)
            {
                result = await Api.GetOrderDetailsAsync(result.OrderId);
            }

            if (result.Result != ExchangeAPIOrderResult.Filled)
            {
                throw new Exception($"{result.ResultCode}: {result.Message}");
            }

            return new BuyResult(result.OrderId, result.AmountFilled, result.AveragePrice);
        }

        public async Task<Dictionary<string, decimal>> GetBalancesAsync()
        {
            return await Api.GetAmountsAvailableToTradeAsync();
        }

        public async Task<decimal> GetPriceAsync(string pair)
        {
            var result = await Api.GetTickerAsync(pair);

            return result.Last;
        }

        public async Task<IEnumerable<string>> GetFiatCurrenciesAsync()
        {
            var symbols = await Api.GetMarketSymbolsAsync();

            var exchangeCurrencies = symbols
                .Select(x => x.ToUpper())
                .Where(x => x.Contains("BTC"))
                .Distinct()
                .Select(x => x.Replace("BTC", ""))
                .ToArray();

            var fiatCurrencies = CultureInfo
                .GetCultures(CultureTypes.SpecificCultures)
                .Where(c => !c.IsNeutralCulture)
                .Select(culture => new RegionInfo(culture.LCID))
                .Select(ri => ri.ISOCurrencySymbol)
                .Concat(ExecutionConsts.StableCoins)
                .Distinct()
                .Select(x => x.ToUpper())
                .ToArray();

            return fiatCurrencies.Intersect(exchangeCurrencies).ToArray();
        }

        public async Task<string> WithdrawAsync(string cryptoCurrency, string address, decimal amount)
        {
            var result = await Api.WithdrawAsync(new ExchangeWithdrawalRequest
            {
                Address = address,
                Amount = amount,
                Currency = cryptoCurrency
            });

            if (!result.Success)
            {
                throw new ScheduleRunFailedException(result.Message);
            }

            return result.Id;
        }

        public void Dispose()
        {
            this.api?.Dispose();
            this.api = null;
        }
    }
}
