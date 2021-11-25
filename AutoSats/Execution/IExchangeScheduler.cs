using AutoSats.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoSats.Execution;

public interface IExchangeScheduler
{
    /// <summary>
    /// Add new schedule.
    /// </summary>
    Task AddScheduleAsync(NewExchangeSchedule schedule, bool runToVerify);

    /// <summary>
    /// Mark schedule as deleted.
    /// </summary>
    Task DeleteScheduleAsync(int id);

    /// <summary>
    /// Pause schedule but don't delete it.
    /// </summary>
    Task PauseScheduleAsync(int id);

    /// <summary>
    /// Resume paused schedule.
    /// </summary>
    Task ResumeScheduleAsync(int id);

    /// <summary>
    /// Get an overview of all exchange schedules.
    /// </summary>
    Task<IEnumerable<ExchangeScheduleSummary>> ListSchedulesAsync();

    /// <summary>
    /// Get schedule details including past events.
    /// </summary>
    Task<ExchangeScheduleDetails> GetScheduleDetailsAsync(int id);

    /// <summary>
    /// Get a list of available symbols with their balances.
    /// </summary>
    Task<IEnumerable<SymbolBalance>> GetSymbolBalancesAsync(string exchange, string key1, string key2, string? key3);
}
