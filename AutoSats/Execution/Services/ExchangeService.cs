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
        private IExchangeAPI? api;
        private readonly ILogger<ExchangeService> logger;

        private IExchangeAPI Api => this.api ?? throw new InvalidOperationException("ExchangeService has not been initialized");

        public ExchangeService(ILogger<ExchangeService> logger)
        {
            this.logger = logger;
        }

        public void Initialize(string exchangeName, string? keysFileName)
        {
            var api = ExchangeAPI.GetExchangeAPI(exchangeName);

            if (keysFileName != null)
            {
                api.LoadAPIKeys(keysFileName);
            }

            this.api = api;
        }

        public void Initialize(string exchangeName, string key1, string key2, string? key3)
        {
            var api = ExchangeAPI.GetExchangeAPI(exchangeName);

            api.LoadAPIKeysUnsecure(key1, key2, key3);

            this.api = api;
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

            // if the order details are missing query them
            if (result.Result == ExchangeAPIOrderResult.Unknown)
            {
                result = await Api.GetOrderDetailsAsync(result.OrderId);
            }

            // query order details until it is fully filled
            while (result.Result == ExchangeAPIOrderResult.FilledPartially || result.Result == ExchangeAPIOrderResult.Pending)
            {
                result = await Api.GetOrderDetailsAsync(result.OrderId);
            }

            if (result.Result != ExchangeAPIOrderResult.Filled && result.AveragePrice == null && result.Price == null)
            {
                throw new Exception($"{result.Result} : {result.ResultCode}: {result.Message}");
            }

            return new BuyResult(result.OrderId, amount, result.AveragePrice ?? result.Price ?? 0);
        }

        public async Task<IEnumerable<Balance>> GetBalancesAsync()
        {
            var balances = await Api.GetAmountsAvailableToTradeAsync();
            
            return balances
                .Select(x => new Balance(x.Key, x.Value))
                .OrderByDescending(x => x.Amount)
                .ToArray();
        }

        public async Task<decimal> GetPriceAsync(string pair)
        {
            var result = await Api.GetTickerAsync(pair);

            return result.Last;
        }

        public async Task<string> WithdrawAsync(string cryptoCurrency, string address, decimal amount)
        {
            var result = await Api.WithdrawAsync(new ExchangeWithdrawalRequest
            {
                Address = address,
                Amount = amount,
                Currency = cryptoCurrency,
            });

            if (!result.Success && string.IsNullOrEmpty(result.Id))
            {
                throw new ScheduleRunFailedException(result.Message);
            }

            return result.Id ?? "unknown";
        }

        public async Task<decimal> GetWithdrawalFeeAsync(string currency)
        {
            var c = currency.ToLower();
            var fees = await Api.GetFeesAync();
            var currencyFees = fees.Where(x => x.Key.ToLower().Contains(c)).ToArray();

            if (currencyFees.Length > 1)
            {
                currencyFees = currencyFees.Where(x => x.Key.ToLower().Contains("withdraw")).ToArray();
            }

            if (currencyFees.Length == 0)
            {
                return 0;
            }

            // return conservatively highest found fee
            return currencyFees.Max(x => x.Value);
        }

        public void Dispose()
        {
            this.api?.Dispose();
            this.api = null;
        }
    }
}
