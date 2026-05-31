using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Deliveries.DTOs;
using Wasel.Api.Modules.Deliveries.Services;

namespace Wasel.Api.Modules.Deliveries.Controllers;

[ApiController]
[Route("api/deliveries")]
[Authorize(Policy = "ActiveUserOnly")]
public class DeliveriesController : ControllerBase
{
    private readonly IDeliveryService _deliveryService;

    public DeliveriesController(IDeliveryService deliveryService)
    {
        _deliveryService = deliveryService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDelivery([FromBody] CreateDeliveryRequestDto request)
    {
        try
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(keycloakId))
            {
                return Unauthorized(new
                {
                    message = "Invalid token: Keycloak user id not found."
                });
            }

            var result = await _deliveryService.CreateDeliveryAsync(request, keycloakId);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableDeliveries(
        [FromQuery] double? latitude,
        [FromQuery] double? longitude,
        [FromQuery] double? radiusKm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (!latitude.HasValue || !longitude.HasValue)
            {
                return BadRequest(new
                {
                    message = "Latitude and longitude are required."
                });
            }

            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(keycloakId))
            {
                return Unauthorized(new
                {
                    message = "Invalid token: Keycloak user id not found."
                });
            }

            var result = await _deliveryService.GetAvailableDeliveriesAsync(
                keycloakId,
                latitude.Value,
                longitude.Value,
                radiusKm,
                page,
                pageSize
            );

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    [HttpPost("{id}/response")]
    public async Task<IActionResult> RespondToDelivery(Guid id, [FromBody] DeliveryResponseDto dto)
    {
        var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(keycloakId))
        {
            return Unauthorized(new { message = "Invalid token: Keycloak user id not found." });
        }

        
        var result = await _deliveryService.RespondToDeliveryAsync(id, keycloakId, dto.Accept);

        if (!result.Success)
        {
            if (result.Message.Contains("not found"))
                return NotFound(new { success = false, message = result.Message });

            if (result.Message.Contains("already assigned"))
                return Conflict(new { success = false, message = result.Message });

            return StatusCode(500, new { success = false, message = result.Message });
        }

        return Ok(new { success = true, message = result.Message, status = result.Status.ToString() });
    }


    [HttpPatch("{id}/status")]
    [Authorize(Roles = "DRIVER")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateDeliveryStatusRequestDto dto)
    {
        var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue("sub");

        if (string.IsNullOrWhiteSpace(keycloakId))
            return Unauthorized(new { message = "Invalid token: Keycloak user id not found." });

        try
        {
            var result = await _deliveryService.UpdateDeliveryStatusAsync(id, keycloakId, dto.NewStatus, dto.Note);

            if (!result.Success)
            {
                if (result.Message.Contains("not found"))
                    return NotFound(new { success = false, message = result.Message });

                if (result.Message.Contains("not assigned"))
                    return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = result.Message });


                if (result.Message.Contains("invalid transition"))
                    return BadRequest(result.Message);

                return StatusCode(500, new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message, status = result.Status.ToString() });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
    

    
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelDelivery(Guid id, [FromBody] CancelDeliveryRequestDto dto)
    {
        try
        {
            var keycloakId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                            ?? User.FindFirstValue("sub");
            if (keycloakId == null)
                return Unauthorized("Utilisateur non authentifié.");

            var currentUser = await _deliveryService.GetUserByKeycloakIdAsync(keycloakId);
            if (currentUser == null)
                return Unauthorized("Utilisateur introuvable.");

            
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            await _deliveryService.CancelDeliveryAsync(id, currentUser, roles, dto.Reason);

            return Ok(new { message = "Livraison annulée avec succès." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Livraison introuvable." });
        }
        catch (UnauthorizedAccessException ex)
    {
        return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
    }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Erreur serveur.", detail = ex.Message });
        }
    }
    
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetDeliveryDetail([FromRoute] Guid id)
    {
        var keycloakId = User.FindFirstValue("sub") 
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(keycloakId))
            return Unauthorized(new { message = "Token invalide." });

        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        try
        {
            var result = await _deliveryService.GetDeliveryDetailAsync(id, keycloakId, roles);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpGet("my-missions")]
    [Authorize]
    public async Task<IActionResult> GetMyMissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var keycloakId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(keycloakId))
            return Unauthorized(new { message = "Token invalide." });

        var result = await _deliveryService.GetMyMissionsAsync(keycloakId, page, pageSize);

        return Ok(result);
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyDeliveries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var keycloakId = User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(keycloakId))
            return Unauthorized(new { message = "Token invalide." });

        var result = await _deliveryService.GetMyDeliveriesAsync(keycloakId, page, pageSize);

        return Ok(result);
    }

    [HttpGet("/api/admin/deliveries")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAdminDeliveries(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var result = await _deliveryService.GetAdminDeliveriesAsync(
                page,
                pageSize,
                search,
                status,
                startDate,
                endDate);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}