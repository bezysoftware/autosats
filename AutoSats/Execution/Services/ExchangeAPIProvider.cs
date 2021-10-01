using ExchangeSharp;
using System.Threading.Tasks;

namespace AutoSats.Execution.Services
{
    public class ExchangeAPIProvider : IExchangeAPIProvider
    {
        public IExchangeAPI GetApi(string name)
        {
            // offload getting the API to the threadpool
            // this is needed for some exchanges which have async initialization logic,
            // which is unfortunately called with .Sync(), which blocks the calling (UI) thread
            // todo: refactor to async once this is released: https://github.com/jjxtra/ExchangeSharp/issues/666
            return Task.Factory.StartNew(() => ExchangeAPI.GetExchangeAPI(name), TaskCreationOptions.LongRunning).Result;
        }
    }
}
