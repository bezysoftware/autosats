namespace AutoSats.Models;

public record ExchangeScheduleDetails(
    ExchangeScheduleSummary Summary,
    IEnumerable<ExchangeEvent> Events)
{
}
