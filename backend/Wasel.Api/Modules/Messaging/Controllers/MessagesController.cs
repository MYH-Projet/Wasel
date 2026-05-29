using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wasel.Api.Modules.Messaging.Services;

namespace Wasel.Api.Modules.Messaging.Controllers;

[ApiController]
[Route("api/deliveries")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessagingService _messagingService;

    public MessagesController(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> GetMessageHistory(Guid id)
    {
        var keycloakId = User.FindFirstValue("sub")
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.Claims.FirstOrDefault(c => c.Type.EndsWith("/nameidentifier"))?.Value;

        if (keycloakId is null)
            return Unauthorized(new { message = "Utilisateur non authentifié." });

        try
        {
            var result = await _messagingService.GetMessageHistoryAsync(id, keycloakId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}