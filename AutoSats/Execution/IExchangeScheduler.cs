using AutoSats.Data;
using AutoSats.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoSats.Execution
{
    public interface IExchangeScheduler
    {
        /// <summary>
        /// Add new schedule.
        /// </summary>
        /// <returns></returns>
        Task AddScheduleAsync(NewExchangeSchedule schedule);

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
        Task<ExchangeSchedule> GetScheduleDetailAsync(int id);
    }
}
