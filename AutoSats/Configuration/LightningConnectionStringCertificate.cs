using BTCPayServer.Lightning;

namespace AutoSats.Configuration
{
    public class LightningConnectionStringCertificate : LightningConnectionString
    {
        public string? CertificatePath { get; set; }
    }
}
