using CronExpressionDescriptor;
using Quartz;

namespace AutoSats.Models;

public record ExchangeScheduleSummary(
    int Id,
    string Exchange,
    string Cron,
    bool IsPaused,
    decimal Spend,
    string SpendCurrency,
    decimal WithdrawalLimit,
    string? WithdrawalAddress,
    DateTime Start,
    ExchangeWithdrawalType WithdrawalType) : IExchange
{
    private DateTime? nextOccurrence;
    private string? cronDescription;

    public decimal TotalSpent { get; set; }

    public decimal TotalAccumulated { get; set; }

    public decimal? AvailableSpend { get; set; }

    public decimal? AvailableBTC { get; set; }


    public DateTime NextOccurrence => this.nextOccurrence ??= GetNextOccurrence();

    public string CronDescription => this.cronDescription ??= GetCronDescription();

    string IExchange.Name => Exchange;

    private string GetCronDescription()
    {
        return ExpressionDescriptor.GetDescription(Cron, new Options { Locale = "en", Use24HourTimeFormat = true });
    }

    private DateTime GetNextOccurrence()
    {
        var after = Start > DateTime.UtcNow ? Start : DateTime.UtcNow;
        return new CronExpression(Cron).GetNextValidTimeAfter(after).GetValueOrDefault(DateTimeOffset.MinValue).UtcDateTime.ToLocalTime();
    }
}
