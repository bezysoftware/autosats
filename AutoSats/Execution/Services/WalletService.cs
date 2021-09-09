using NBitcoin.RPC;
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
            var request = new GetNewAddressRequest { Label = "AutoSats" };
            var address = await this.client.GetNewAddressAsync(request);

            return address.ToString();
        }
    }
}
