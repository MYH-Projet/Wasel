using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Deliveries.Enums;

namespace Wasel.Api.Modules.Deliveries.Repositories;

public interface IDeliveryRepository
{
    Task<User?> GetUserByKeycloakIdAsync(string keycloakId);

    Task<bool> UserHasClientActiveModeAsync(Guid userId);

    Task<Delivery> CreateDeliveryRequestAsync(
        Address pickupAddress,
        Address dropoffAddress,
        Parcel parcel,
        Delivery delivery,
        List<DeliveryStatusHistory> histories);

    Task<bool> UserHasDriverActiveModeAsync(Guid userId);

    Task<bool> UserHasApprovedDriverAsync(Guid userId);

    IQueryable<Delivery> GetAvailableDeliveriesQuery();

    Task<Delivery?> GetByIdAsync(Guid id);

    Task UpdateAsync(Delivery delivery);

    Task AddStatusHistoryAsync(DeliveryStatusHistory history);

    Task CancelDeliveryAsync(Delivery delivery, DeliveryStatus newStatus, string? reason);

    Task<Delivery?> GetDeliveryWithDetailsAsync(Guid deliveryId);

    Task<List<Delivery>> GetDriverMissionsAsync(Guid driverId, int page, int pageSize);

    Task<int> CountDriverMissionsAsync(Guid driverId);

    Task<List<Delivery>> GetClientDeliveriesAsync(Guid clientId, int page, int pageSize);
    
    Task<int> CountClientDeliveriesAsync(Guid clientId);

    Task<(List<Delivery> Items, int TotalCount)> GetAdminDeliveriesAsync(
    int page,
    int pageSize,
    string? search,
    List<DeliveryStatus>? statuses,
    DateTime? startDate,
    DateTime? endDate);

    Task<List<Delivery>> GetActiveDeliveriesForUserAsync(
        Guid userId, 
        Guid? driverId, 
        IEnumerable<DeliveryStatus> activeStatuses);
}
