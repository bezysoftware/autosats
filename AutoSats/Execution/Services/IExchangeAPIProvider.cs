using ExchangeSharp;

namespace AutoSats.Execution.Services;

public interface IExchangeAPIProvider
{
    Task<IExchangeAPI> GetApiAsync(string name);
}
