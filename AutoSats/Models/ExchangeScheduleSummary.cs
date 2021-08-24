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
        ExchangeWithdrawalType WithdrawalType)
    {
        private DateTime? nextOccurrence;
        private string? cronDescription;

        public DateTime NextOccurence => this.nextOccurrence ??= GetNextOccurence();

        public string CronDescription => this.cronDescription ??= GetCronDescription();


        private string GetCronDescription()
        {
            return ExpressionDescriptor.GetDescription(Cron, new Options { Locale = "en" });
        }

        private DateTime GetNextOccurence()
        {
            return new CronExpression(Cron).GetNextValidTimeAfter(DateTimeOffset.UtcNow).GetValueOrDefault(DateTimeOffset.MinValue).UtcDateTime.ToLocalTime();
        }
    }
}
