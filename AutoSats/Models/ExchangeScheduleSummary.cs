using AutoSats.Data;
using CronExpressionDescriptor;
using Quartz;
using System;

namespace AutoSats.Models
{
    public record ExchangeScheduleSummary(
        int Id,
        string Exchange,
        string Cron,
        bool IsPaused,
        decimal Spend,
        string CurrencyPair,
        DateTime Start,
        ExchangeWithdrawalType WithdrawalType) : IExchange
    {
        private DateTime? nextOccurrence;
        private string? cronDescription;

        public decimal TotalSpent { get; set; }

        public decimal TotalAccumulated { get; set; }

        public DateTime NextOccurence => this.nextOccurrence ??= GetNextOccurence();

        public string CronDescription => this.cronDescription ??= GetCronDescription();

        string IExchange.Name => Exchange;

        private string GetCronDescription()
        {
            return ExpressionDescriptor.GetDescription(Cron, new Options { Locale = "en", Use24HourTimeFormat = true });
        }

        private DateTime GetNextOccurence()
        {
            return new CronExpression(Cron).GetNextValidTimeAfter(Start).GetValueOrDefault(DateTimeOffset.MinValue).UtcDateTime.ToLocalTime();
        }
    }
}
