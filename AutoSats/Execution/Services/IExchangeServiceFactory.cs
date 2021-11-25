using System.Threading.Tasks;

namespace AutoSats.Execution.Services;

public interface IExchangeServiceFactory
{
    /// <summary>
    /// Get <see cref="IExchangeService"/> initialized with keys from a protected file.
    /// </summary>
    Task<IExchangeService> GetServiceAsync(string exchangeName, string? fileName);

    /// <summary>
    /// Get <see cref="IExchangeService"/> initialized with specified keys.
    /// </summary>
    Task<IExchangeService> GetServiceAsync(string exchangeName, string key1, string key2, string? key3);
}
