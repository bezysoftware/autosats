using System.Threading.Tasks;

namespace AutoSats.Execution
{
    public interface IExchangeScheduleRunner
    {
        /// <summary>
        /// Run specified exchange schedule.
        /// </summary>
        Task RunScheduleAsync(int id);

        /// <summary>
        /// Folder where api keys files are to be placed.
        /// </summary>
        string KeysPath { get; }
    }
}
