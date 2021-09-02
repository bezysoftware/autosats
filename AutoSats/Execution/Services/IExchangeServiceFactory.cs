namespace AutoSats.Execution.Services
{
    public interface IExchangeServiceFactory
    {
        /// <summary>
        /// Get <see cref="IExchangeService"/> initialized without any keys. Only public methods will be available. 
        /// </summary>
        IExchangeService GetService(string exchangeName);

        /// <summary>
        /// Get <see cref="IExchangeService"/> initialized with keys from a protected file.
        /// </summary>
        IExchangeService GetService(string exchangeName, string? fileName);

        /// <summary>
        /// Get <see cref="IExchangeService"/> initialized with specified keys.
        /// </summary>
        IExchangeService GetService(string exchangeName, string key1, string key2, string? key3);
    }
}
