using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Infrastructure.Keycloak;
using Wasel.Api.Modules.Auth.Services;
using Wasel.Api.Modules.Tracking.DTOs;
using Wasel.Api.Modules.Tracking.Repositories;
using Wasel.Api.Modules.Tracking.Services;

namespace Wasel.Api.Modules.Tracking.Controllers;

[ApiController]
[Route("api/tracking")]
[Authorize]
public class TrackingController : ControllerBase
{
    private readonly ITrackingService _trackingService;
    private readonly ITrackingRepository _trackingRepository;
    private readonly IAuthService _authService;

    public TrackingController(ITrackingService trackingService, ITrackingRepository trackingRepository, IAuthService authService)
    {
        _trackingService = trackingService;
        _trackingRepository = trackingRepository;
        _authService = authService;
    }

    [HttpGet("deliveries/{deliveryId}/last-position")]
    public async Task<ActionResult<TrackingPointResponseDto>> GetLastPositionByDelivery(Guid deliveryId)
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        
        var delivery = await _trackingRepository.GetDeliveryByIdAsync(deliveryId);
        if (delivery == null)
        {
            return NotFound(new { message = "Livraison introuvable." });
        }

        bool isAuthorized = false;

        if (currentUser.Roles.Contains(KeycloakConstants.RoleAdmin))
        {
            isAuthorized = true;
        }
        else if (currentUser.Roles.Contains(KeycloakConstants.RoleClient) && delivery.ClientId == currentUser.LocalUserId)
        {
            isAuthorized = true;
        }
        else if (currentUser.Roles.Contains(KeycloakConstants.RoleDriver))
        {
            var driver = await _trackingRepository.GetDriverByUserIdAsync(currentUser.LocalUserId!.Value);
            if (driver != null && delivery.DriverId == driver.Id)
            {
                isAuthorized = true;
            }
        }

        if (!isAuthorized)
        {
            return Forbid();
        }

        var position = await _trackingService.GetLastPositionByDeliveryIdAsync(deliveryId);
        if (position == null)
        {
            return NotFound(new { message = "Aucune position trouvée pour cette livraison." });
        }

        return Ok(position);
    }

    [HttpGet("drivers/{driverId}/last-position")]
    public async Task<ActionResult<TrackingPointResponseDto>> GetLastPositionByDriver(Guid driverId)
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        
        bool isAuthorized = false;

        if (currentUser.Roles.Contains(KeycloakConstants.RoleAdmin))
        {
            isAuthorized = true;
        }
        else if (currentUser.Roles.Contains(KeycloakConstants.RoleDriver))
        {
            var driver = await _trackingRepository.GetDriverByUserIdAsync(currentUser.LocalUserId!.Value);
            if (driver != null && driver.Id == driverId)
            {
                isAuthorized = true;
            }
        }
        
        // Clients n'ont normalement pas le droit de voir un livreur arbitraire, seulement via leur delivery

        if (!isAuthorized)
        {
            return Forbid();
        }

        var position = await _trackingService.GetLastPositionByDriverIdAsync(driverId);
        if (position == null)
        {
            return NotFound(new { message = "Aucune position trouvée pour ce livreur." });
        }

        return Ok(position);
    }

    [HttpGet("me/last-position")]
    [Authorize(Policy = "DriverOnly")]
    public async Task<ActionResult<TrackingPointResponseDto>> GetMyLastPosition()
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        
        var driver = await _trackingRepository.GetDriverByUserIdAsync(currentUser.LocalUserId!.Value);
        if (driver == null)
        {
            return Forbid();
        }

        var position = await _trackingService.GetLastPositionByDriverIdAsync(driver.Id);
        if (position == null)
        {
            return NotFound(new { message = "Aucune position trouvée pour votre compte." });
        }

        return Ok(position);
    }
}
