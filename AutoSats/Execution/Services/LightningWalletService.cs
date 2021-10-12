using BTCPayServer.Lightning;
using System.Threading.Tasks;

namespace AutoSats.Execution.Services
{
    public class LightningWalletService : IWalletService
    {
        private readonly ILightningClient client;

        public LightningWalletService(ILightningClient client)
        {
            this.client = client;
        }

        public async Task<string> GenerateDepositAddressAsync()
        {
            var address = await this.client.GetDepositAddress();

            return address.ToString();
        }
    }
}
