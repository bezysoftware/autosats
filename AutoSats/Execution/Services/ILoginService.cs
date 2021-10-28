using System.Threading.Tasks;

namespace AutoSats.Execution.Services
{
    public interface ILoginService
    {
        Task<bool> LoginAsync(string? password);
    }
}
