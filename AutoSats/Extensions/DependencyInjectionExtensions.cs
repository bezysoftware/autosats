using AutoSats.Configuration;
using AutoSats.Execution.Services;
using BTCPayServer.Lightning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NBitcoin;
using NBitcoin.RPC;

namespace AutoSats.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddWalletServices(this IServiceCollection services, WalletType walletType)
        {
            switch (walletType)
            {
                case WalletType.Bitcoind:
                    services.AddBitcoinRPC();
                    break;
                case WalletType.Lightning:
                    services.AddBitcoinLightning();
                    break;
            }

            return services;
        }

        public static IServiceCollection AddBitcoinLightning(this IServiceCollection services)
        {
            services.AddScoped<IWalletService, LightningWalletService>();

            services
                .AddOptions<LightningConnectionString>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection("Wallet:Lightning").Bind(options));

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<LightningConnectionString>>();
                return new LightningClientFactory(Network.Main).Create(options.Value);
            });

            return services;
        }

        public static IServiceCollection AddBitcoinRPC(this IServiceCollection services)
        {
            services.AddScoped<IWalletService, BitcoinWalletService>();

            services
                .AddOptions<BitcoinOptions>()
                .Configure<IConfiguration>((options, configuration) => configuration.GetSection("Wallet:Bitcoind").Bind(options));

            services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<BitcoinOptions>>();
                return new RPCClient(options.Value.Auth, options.Value.Url, Network.Main);
            });

            return services;
        }
    }
}
