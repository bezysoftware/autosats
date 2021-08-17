using System.Threading.Tasks;

namespace AutoSats.Execution
{
    public interface IExchangeScheduleRunner
    {
        /// <summary>
        /// Run specified exchange schedule.
        /// </summary>
        Task RunScheduleAsync(int id);

        string KeysPath { get; }
    }
}
