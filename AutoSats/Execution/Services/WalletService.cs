using NBitcoin.RPC;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSats.Execution.Services
{
    public class WalletService : IWalletService
    {
        private readonly RPCClient client;

        public WalletService(RPCClient client)
        {
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
                await this.client.LoadWalletAsync(wallets.Wallets[0].Name);
            }
            else
            {
                await this.client.CreateWalletAsync("autosats");
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
