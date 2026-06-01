using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Wasel.Api.Infrastructure.Keycloak;
using Wasel.Api.Modules.Auth.Services;
using Wasel.Api.Modules.Tracking.DTOs;
using Wasel.Api.Modules.Tracking.Repositories;
using Wasel.Api.Modules.Tracking.Services;

namespace Wasel.Api.Modules.Tracking.Hubs;

[Authorize(Policy = "ActiveUserOnly")]
public class GpsHub : Hub
{
    private readonly ITrackingService _trackingService;
    private readonly IAuthService _authService;
    private readonly ITrackingRepository _trackingRepository;

    public GpsHub(ITrackingService trackingService, IAuthService authService, ITrackingRepository trackingRepository)
    {
        _trackingService = trackingService;
        _authService = authService;
        _trackingRepository = trackingRepository;
    }

    public override async Task OnConnectedAsync()
    {
        // Auto-sync au démarrage de la connexion WebSocket
        // Si l'utilisateur n'existe pas en local, il est créé. S'il existe, on l'a récupéré.
        try
        {
            await _authService.EnsureCurrentUserExistsAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            // Ignorer l'erreur de concurrence : 
            // L'utilisateur a déjà été mis à jour par une requête API exécutée exactement en même temps.
        }
        
        await base.OnConnectedAsync();
    }

    public async Task SendPosition(TrackingPointUpdateDto dto)
    {
        // La vérification des droits (rôle DRIVER et compte validé) est gérée par le TrackingService
        var response = await _trackingService.UpdateCurrentDriverPositionAsync(dto);

        // Toujours envoyer la réponse au livreur (confirmation)
        await Clients.Caller.SendAsync("ReceivePosition", response);

        // Si la position est liée à une livraison en cours, on l'envoie aussi au groupe spécifique
        // (en excluant le caller pour éviter le doublon)
        if (response.DeliveryId.HasValue)
        {
            await Clients.GroupExcept($"delivery-{response.DeliveryId.Value}", Context.ConnectionId)
                .SendAsync("ReceivePosition", response);
        }
    }

    public async Task JoinDeliveryGroup(Guid deliveryId)
    {
        var currentUser = await _authService.EnsureCurrentUserExistsAsync();
        
        var delivery = await _trackingRepository.GetDeliveryByIdAsync(deliveryId);
        if (delivery == null)
        {
            throw new HubException("Livraison introuvable.");
        }

        bool isAuthorized = false;

        // ADMIN : Toujours autorisé
        if (currentUser.Roles.Contains(KeycloakConstants.RoleAdmin))
        {
            isAuthorized = true;
        }
        // CLIENT : Seulement s'il est le propriétaire
        else if (currentUser.Roles.Contains(KeycloakConstants.RoleClient))
        {
            if (delivery.ClientId == currentUser.LocalUserId)
            {
                isAuthorized = true;
            }
        }
        // DRIVER : Seulement s'il est affecté à la course
        else if (currentUser.Roles.Contains(KeycloakConstants.RoleDriver))
        {
            // Fetch Driver by UserId to check if the driverId matches
            var driver = await _trackingRepository.GetDriverByUserIdAsync(currentUser.LocalUserId!.Value);
            if (driver != null && delivery.DriverId == driver.Id)
            {
                isAuthorized = true;
            }
        }

        if (!isAuthorized)
        {
            throw new HubException("Vous n'êtes pas autorisé à suivre cette livraison.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"delivery-{deliveryId}");
    }

    public async Task LeaveDeliveryGroup(Guid deliveryId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"delivery-{deliveryId}");
    }
}
