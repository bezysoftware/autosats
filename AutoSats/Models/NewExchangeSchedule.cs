using AutoSats.Data;
using System;

namespace AutoSats.Models;

public record NewExchangeSchedule(
    SymbolBalance Amount,
    string Exchange,
    string[] Keys,
    string Cron,
    DateTime Start,
    ExchangeWithdrawalType WithdrawalType,
    string? WithdrawalAddress,
    decimal? WithdrawalLimit)
{ }
