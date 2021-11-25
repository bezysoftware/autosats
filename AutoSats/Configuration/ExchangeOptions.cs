using AutoSats.Models;
using System;

namespace AutoSats.Configuration;

public class ExchangeOptions : IExchange
{
    public string Name { get; init; } = string.Empty;

    public string Key1Name { get; init; } = string.Empty;

    public string Key2Name { get; init; } = string.Empty;

    public string Key3Name { get; init; } = string.Empty;

    public string ApiUrl { get; init; } = string.Empty;

    public string ApiName { get; init; } = string.Empty;

    public string Hint { get; init; } = string.Empty;

    public bool ReverseCurrencies { get; init; }

    public BuyOrderType BuyOrderType { get; set; } = BuyOrderType.Market;

    public string BitcoinSymbol { get; set; } = "BTC";

    public WithdrawalType WithdrawalType { get; set; }

    public char[] TickerPrefixes { get; set; } = Array.Empty<char>();

    public string[] Permissions { get; init; } = Array.Empty<string>();
}
