using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Auth.Services;
using Wasel.Api.Modules.Auth.DTOs;
using Wasel.Api.Modules.Users.DTOs;

namespace Wasel.Api.Modules.Auth.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponseDto>> GetCurrentUser()
    {
        var user = await _authService.GetCurrentUserAsync();
        return Ok(user);
    }

    [HttpPost("sync")]
    public async Task<ActionResult<UserResponseDto>> SyncUser()
    {
        var user = await _authService.SyncCurrentUserAsync();
        return Ok(user);
    }

    [HttpGet("claims")]
    public IActionResult GetRawClaims([FromServices] IWebHostEnvironment env)
    {
        if (!env.IsDevelopment())
        {
            return NotFound();
        }

        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(claims);
    }
}
