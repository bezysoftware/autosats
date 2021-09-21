using ExchangeSharp;

namespace AutoSats.Execution.Services
{
    public interface IExchangeAPIProvider
    {
        IExchangeAPI GetApi(string name);
    }
}
