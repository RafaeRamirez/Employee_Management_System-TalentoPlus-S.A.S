using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Application.Contracts.Requests;
using TalentoPlus.Application.Interfaces;

namespace TalentoPlus.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        // Self-registration returns a JWT so the employee can consume protected endpoints right away.
        var token = await _authService.RegisterEmployeeAsync(request, cancellationToken);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var token = await _authService.LoginAsync(request, cancellationToken);
        return Ok(new { token });
    }
}
