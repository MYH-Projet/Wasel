using Wasel.Api.Modules.Deliveries.DTOs;
using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Enums;
namespace Wasel.Api.Modules.Deliveries.Services;

public interface IDeliveryService
{
    Task<CreateDeliveryResponseDto> CreateDeliveryAsync(
        CreateDeliveryRequestDto request,
        string keycloakId);

    Task<PagedResultDto<AvailableDeliveryDto>> GetAvailableDeliveriesAsync(
        string keycloakId,
        double latitude,
        double longitude,
        double? radiusKm,
        int page,
        int pageSize);

    Task<(bool Success, string Message, DeliveryStatus Status)> RespondToDeliveryAsync(
            Guid deliveryId, Guid driverId, bool accept);

    Task<(bool Success, string Message, DeliveryStatus Status)> UpdateDeliveryStatusAsync(
    Guid deliveryId, Guid driverId, DeliveryStatus newStatus, string? note);

    Task<User?> GetUserByKeycloakIdAsync(string keycloakId);

    Task CancelDeliveryAsync(Guid deliveryId, User currentUser, List<string> roles, string? reason);

    Task<DeliveryDetailResponseDto> GetDeliveryDetailAsync(  // ← ajouté
        Guid deliveryId,
        string keycloakId,
        IEnumerable<string> roles);

    Task<object> GetMyMissionsAsync(string keycloakId, int page, int pageSize);

    Task<object> GetMyDeliveriesAsync(string keycloakId, int page, int pageSize);

}