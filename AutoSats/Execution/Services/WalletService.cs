using Microsoft.Extensions.Logging;
using NBitcoin.RPC;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSats.Execution.Services
{
    public class WalletService : IWalletService
    {
        private readonly ILogger<WalletService> logger;
        private readonly RPCClient client;

        public WalletService(ILogger<WalletService> logger, RPCClient client)
        {
            this.logger = logger;
            this.client = client;
        }

        public async Task<string> GenerateDepositAddressAsync()
        {
            try
            {
                return await GetNewAddressAsync();
            }
            catch (RPCException ex) when (ex.RPCCode == RPCErrorCode.RPC_WALLET_NOT_FOUND)
            {
                this.logger.LogWarning(ex, "Wallet is not loaded, trying to either load it or create a new one");
                await LoadOrCreateWalletAsync();
                return await GetNewAddressAsync();
            }
        }

        private async Task LoadOrCreateWalletAsync()
        {
            var response = await this.client.SendCommandAsync("listwalletdir");
            var wallets = JsonConvert.DeserializeObject<WalletsResponse>(response.ResultString);

            if (wallets.Wallets.Any())
            {
                this.logger.LogInformation($"Loading wallet {wallets.Wallets[0].Name}");
                await this.client.LoadWalletAsync(wallets.Wallets[0].Name);
            }
            else
            {
                this.logger.LogInformation("Creating new wallet for AutoSats");
                await this.client.CreateWalletAsync("AutoSats");
            }
        }

        private async Task<string> GetNewAddressAsync()
        {
            var request = new GetNewAddressRequest { Label = "AutoSats" };
            var address = await this.client.GetNewAddressAsync(request);
            return address.ToString();
        }

        internal class WalletsResponse
        {
            [JsonProperty("wallets")]
            public WalletResponse[] Wallets { get; set; } = Array.Empty<WalletResponse>();
        }

        internal class WalletResponse
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}
