using System.Threading.Tasks;

namespace AutoSats.Execution.Services;

public interface IWalletService
{
    Task<string> GenerateDepositAddressAsync();
}
