using AutoSats.Data;
using System;

namespace AutoSats.Models
{
    public record NewExchangeSchedule(
        string CurrencyPair,
        string Exchange,
        string[] Keys,
        string Cron,
        DateTime Start,
        decimal Spend,
        ExchangeWithdrawalType WithdrawalType,
        string? WithdrawalAddress,
        decimal? WithdrawalLimit)
    { }
}
