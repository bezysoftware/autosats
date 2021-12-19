namespace AutoSats.Execution.Services;

public interface ILoginService
{
    Task<bool> LoginAsync(string? password);
    Task LogoutAsync();
}
