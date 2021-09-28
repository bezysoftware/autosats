using ExchangeSharp;

namespace AutoSats.Execution.Services
{
    public class ExchangeAPIProvider : IExchangeAPIProvider
    {
        public IExchangeAPI GetApi(string name)
        {
            return ExchangeAPI.GetExchangeAPI(name);
        }
    }
}
