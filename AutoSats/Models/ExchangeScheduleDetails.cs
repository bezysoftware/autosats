using AutoSats.Data;
using System.Collections.Generic;

namespace AutoSats.Models
{
    public record ExchangeScheduleDetails(
        ExchangeScheduleSummary Summary,
        IEnumerable<ExchangeEvent> Events)
    {        
    }
}
