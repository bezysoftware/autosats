using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AutoSats.Execution.Services
{
    public class ExchangeServiceFactory : IExchangeServiceFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ExchangeServiceFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<IExchangeService> GetServiceAsync(string exchangeName, string? fileName)
        {
            var service = GetServiceFromProvider();

            await service.InitializeAsync(exchangeName, fileName);

            return service;
        }

        public async Task<IExchangeService> GetServiceAsync(string exchangeName, string key1, string key2, string? key3)
        {
            var service = GetServiceFromProvider();

            await service.InitializeAsync(exchangeName, key1, key2, key3);

            return service;
        }

        private IExchangeService GetServiceFromProvider()
        {
            return this.serviceProvider.GetService<IExchangeService>() ?? throw new InvalidOperationException($"{nameof(IExchangeService)} needs to be registered");
        }
    }
}
