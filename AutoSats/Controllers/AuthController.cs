using AutoSats.Execution.Services;
using AutoSats.Models;
using Microsoft.AspNetCore.Mvc;

namespace AutoSats.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly ILoginService loginService;

    public AuthController(ILoginService loginService)
    {
        this.loginService = loginService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginRequest model)
    {
        if (await this.loginService.LoginAsync(model.Password))
        {
            return LocalRedirect("/");
        }

        return Unauthorized();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await this.loginService.LogoutAsync();
        return LocalRedirect("/");
    }
}
