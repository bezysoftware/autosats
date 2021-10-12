using BTCPayServer.Lightning;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AutoSats.Execution.Services
{
    public class LightningWalletService : IWalletService
    {
        private readonly ILogger<LightningWalletService> logger;
        private readonly ILightningClient client;

        public LightningWalletService(ILogger<LightningWalletService> logger, ILightningClient client)
        {
            this.logger = logger;
            this.client = client;
        }

        public async Task<string> GenerateDepositAddressAsync()
        {
            try
            {
                var address = await this.client.GetDepositAddress();

                return address.ToString();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to retrieve lightning wallet deposit address");
                throw;
            }
        }
    }
}
