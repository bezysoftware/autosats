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
    ExchangeWithdrawalType WithdrawalType, 
    NotificationType NotificationType) : IExchange
{
    private DateTime? nextOccurrence;
    private string? cronDescription;

    public decimal TotalSpent { get; init; }

    public decimal TotalAccumulated { get; init; }

    public decimal? AvailableSpend { get; init; }

    public decimal? AvailableBTC { get; init; }
    
    public decimal? CurrentPrice { get; init; }

    public decimal? GainLoss => TotalAccumulated * CurrentPrice - TotalSpent;

    public decimal? GainLossPercent => GainLoss / TotalSpent * 100;

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
