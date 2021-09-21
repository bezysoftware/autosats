using AutoSats.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoSats.Execution.Services
{
    public interface IExchangeService
    {
        /// <summary>
        /// Initialize the service with keys coming from a file.
        /// </summary>
        IExchangeService Initialize(string exchangeName, string? keysFileName);

        /// <summary>
        /// Initialize the service with supplied keys.
        /// </summary>
        IExchangeService Initialize(string exchangeName, string key1, string key2, string? key3);
        
        /// <summary>
        /// Gets balances for all your currencies.
        /// </summary>
        Task<IEnumerable<Balance>> GetBalancesAsync();

        /// <summary>
        /// Buys given amount of the left currency in the specified symbol with funds from the right currency in the pair.
        /// </summary>
        Task<BuyResult> BuyAsync(string symbol, decimal amount, BuyOrderType orderType, bool invert);

        /// <summary>
        /// Returns last price for given trading symbol (e.g. "btcusd").
        /// </summary>
        Task<decimal> GetPriceAsync(string symbol);

        /// <summary>
        /// Withdraws given amount to target address. Returns withdrawal transaction id.
        /// </summary>
        Task<string> WithdrawAsync(string cryptoCurrency, string address, decimal amount);

        /// <summary>
        /// Get withdrawal fee for given currency.
        /// </summary>
        Task<decimal> GetWithdrawalFeeAsync(string currency);

        /// <summary>
        /// Get a list of available trading symbols where the given currency is present.
        /// </summary>
        Task<IEnumerable<Symbol>> GetSymbolsWithAsync(string currency);
    }
}