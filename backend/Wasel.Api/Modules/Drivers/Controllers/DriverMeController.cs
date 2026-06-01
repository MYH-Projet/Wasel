using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Drivers.DTOs;
using Wasel.Api.Modules.Drivers.Services;
using Wasel.Api.Shared.Exceptions;
using Wasel.Api.Shared.Security;

namespace Wasel.Api.Modules.Drivers.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize(Policy = "ActiveUserOnly")]
public class DriverMeController : ControllerBase
{
    private readonly IDriverService _driverService;
    private readonly ICurrentUserService _currentUserService;

    public DriverMeController(IDriverService driverService, ICurrentUserService currentUserService)
    {
        _driverService = driverService;
        _currentUserService = currentUserService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<DriverMeResponseDto>> RegisterDriver([FromBody] RegisterDriverRequestDto request)
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized(new { message = "Invalid token: missing subject claim" });
        }

        try
        {
            var driverProfile = await _driverService.RegisterCurrentUserAsDriverAsync(keycloakId, request);
            // Even though it's a creation, since the URL does not exist to get it specifically by its ID in this API scope, returning Ok or Created is fine.
            return Ok(driverProfile);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    public async Task<ActionResult<DriverMeResponseDto>> GetMyProfile()
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized(new { message = "Invalid token: missing subject claim" });
        }

        try
        {
            var driverProfile = await _driverService.GetCurrentDriverProfileAsync(keycloakId);
            return Ok(driverProfile);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPost("dossier/submit")]
    public async Task<ActionResult<DriverMeResponseDto>> SubmitDossier()
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return Unauthorized(new { message = "Invalid token: missing subject claim" });
        }

        try
        {
            var driverProfile = await _driverService.SubmitCurrentDriverDossierAsync(keycloakId);
            return Ok(driverProfile);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }
}
