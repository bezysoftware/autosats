using ExchangeSharp;
using System.Threading.Tasks;

namespace AutoSats.Execution.Services
{
    public class ExchangeAPIProvider : IExchangeAPIProvider
    {
        public Task<IExchangeAPI> GetApiAsync(string name)
        {
            return ExchangeAPI.GetExchangeAPIAsync(name);
        }
    }
}
