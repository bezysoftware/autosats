using AutoSats.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AutoSats.Execution.Services;

public class LoginService : ILoginService
{
    private readonly ILogger<LoginService> logger;
    private readonly IHttpContextAccessor http;
    private readonly ApplicationOptions options;

    public LoginService(ILogger<LoginService> logger, IOptions<ApplicationOptions> options, IHttpContextAccessor http)
    {
        this.logger = logger;
        this.http = http;
        this.options = options.Value;
    }

    public async Task<bool> LoginAsync(string? password)
    {
        if (string.IsNullOrEmpty(this.options.Password) || password == this.options.Password)
        {
            await this.http.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                });

            return true;
        }

        this.logger.LogError("Authentication failed");

        return false;
    }

    public Task LogoutAsync()
    {
        return this.http.HttpContext!.SignOutAsync();
    }
}
