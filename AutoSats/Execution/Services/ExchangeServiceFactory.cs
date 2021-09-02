using System;
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

        public IExchangeService GetService(string exchangeName)
        {
            var service = GetServiceFromProvider();

            service.Initialize(exchangeName, null);

            return service;
        }

        public IExchangeService GetService(string exchangeName, string? fileName)
        {
            var service = GetServiceFromProvider();

            service.Initialize(exchangeName, fileName);

            return service;
        }

        public IExchangeService GetService(string exchangeName, string key1, string key2, string? key3)
        {
            var service = GetServiceFromProvider();

            service.Initialize(exchangeName, key1, key2, key3);

            return service;
        }

        private IExchangeService GetServiceFromProvider()
        {
            return this.serviceProvider.GetService<IExchangeService>() ?? throw new InvalidOperationException($"{nameof(IExchangeService)} needs to be registered");
        }
    }
}
