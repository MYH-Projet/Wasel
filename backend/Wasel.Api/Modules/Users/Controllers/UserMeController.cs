using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Users.DTOs;
using Wasel.Api.Modules.Users.Services;
using Wasel.Api.Shared.Security;
using Wasel.Api.Shared.Exceptions;

namespace Wasel.Api.Modules.Users.Controllers;

[ApiController]
[Route("api/users/me")]
[Authorize]
public class UserMeController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UserMeController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    [HttpPatch]
    public async Task<ActionResult<UserResponseDto>> UpdateMyProfile([FromBody] UpdateMyProfileRequestDto request)
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized(new { message = "Invalid token: missing subject claim" });
        }

        try
        {
            var updatedProfile = await _userService.UpdateMyProfileAsync(keycloakId, request);
            return Ok(updatedProfile);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPatch("preferences")]
    public async Task<ActionResult<UserPreferencesResponseDto>> UpdateMyPreferences([FromBody] UpdateUserPreferencesRequestDto request)
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized(new { message = "Invalid token: missing subject claim" });
        }

        try
        {
            var updatedPreferences = await _userService.UpdateMyPreferencesAsync(keycloakId, request);
            return Ok(updatedPreferences);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }
}
