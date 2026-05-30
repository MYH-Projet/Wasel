using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Wasel.Api.Modules.Messaging.DTOs;
using Wasel.Api.Modules.Messaging.Services;

namespace Wasel.Api.Modules.Messaging.Hubs;

[Authorize]
public class MessagingHub : Hub
{
    private readonly IMessagingService _messagingService;

    public MessagingHub(IMessagingService messagingService)
    {
        _messagingService = messagingService;
    }

    public async Task JoinDeliveryGroup(Guid deliveryId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"delivery-{deliveryId}");
    }

    public async Task SendMessage(SendMessageRequestDto request)
    {
        var keycloakId = Context.User?.FindFirstValue("sub")
                      ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? Context.User?.Claims.FirstOrDefault(c => c.Type.EndsWith("/nameidentifier"))?.Value;

        if (keycloakId is null)
            throw new HubException("Utilisateur non authentifié.");

        try
        {
            var result = await _messagingService.SendMessageAsync(request, keycloakId);

            await Clients.Group($"delivery-{request.DeliveryId}")
                .SendAsync("ReceiveMessage", result);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            throw new HubException(ex.Message);
        }
    }
}