using AutoSats.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.RPC;

namespace AutoSats.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddBitcoinRPC(this IServiceCollection services)
        {
            services
                .AddOptions<BitcoinOptions>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection("Bitcoin").Bind(options));

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BitcoinOptions>>();
                return new RPCClient(options.Value.Auth, options.Value.Url, Network.Main);
            });

            return services;
        }
    }
}
