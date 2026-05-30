using Wasel.Api.Modules.Tracking.Entities;

namespace Wasel.Api.Modules.Tracking.Repositories;

public interface ITrackingRepository
{
    Task<TrackingPoint?> GetLastPositionByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<TrackingPoint?> GetLastPositionByDeliveryIdAsync(Guid deliveryId, CancellationToken cancellationToken = default);
    Task<TrackingPoint> AddTrackingPointAsync(TrackingPoint point, CancellationToken cancellationToken = default);
    Task<Wasel.Api.Modules.Drivers.Entities.Driver?> GetDriverByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Wasel.Api.Modules.Deliveries.Entities.Delivery?> GetDeliveryByIdAsync(Guid deliveryId, CancellationToken cancellationToken = default);
}
