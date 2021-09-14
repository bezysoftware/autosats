﻿using AutoSats.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoSats.Execution.Services
{
    public interface IExchangeService
    {
        /// <summary>
        /// Initialize the service with keys coming from a file.
        /// </summary>
        void Initialize(string exchangeName, string? keysFileName);

        /// <summary>
        /// Initialize the service with supplied keys.
        /// </summary>
        void Initialize(string exchangeName, string key1, string key2, string? key3);
        
        /// <summary>
        /// Gets balances for all your currencies.
        /// </summary>
        Task<Dictionary<string, decimal>> GetBalancesAsync();

        /// <summary>
        /// Buys given amount of the left currency in the specified pair with funds from the right currency in the pair.
        /// </summary>
        Task<BuyResult> BuyAsync(string pair, decimal amount);

        /// <summary>
        /// Returns last price for given trading pair (e.g. "btcusd").
        /// </summary>
        Task<decimal> GetPriceAsync(string pair);

        /// <summary>
        /// Withdraws given amount to target address. Returns withdrawal transaction id.
        /// </summary>
        Task<string> WithdrawAsync(string cryptoCurrency, string address, decimal amount);

        /// <summary>
        /// Gets a list of fiat currency symbols.
        /// </summary>
        Task<IEnumerable<string>> GetFiatCurrenciesAsync();

        /// <summary>
        /// Get withdrawal fee for given currency.
        /// </summary>
        Task<decimal> GetWithdrawalFeeAsync(string currency);
    }
}