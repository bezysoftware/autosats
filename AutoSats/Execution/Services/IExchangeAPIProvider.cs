using ExchangeSharp;
using System.Threading.Tasks;

namespace AutoSats.Execution.Services;

public interface IExchangeAPIProvider
{
    Task<IExchangeAPI> GetApiAsync(string name);
}
