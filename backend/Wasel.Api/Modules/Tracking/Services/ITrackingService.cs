using Wasel.Api.Modules.Tracking.DTOs;

namespace Wasel.Api.Modules.Tracking.Services;

public interface ITrackingService
{
    Task<TrackingPointResponseDto> UpdateCurrentDriverPositionAsync(TrackingPointUpdateDto dto, CancellationToken cancellationToken = default);
    Task<TrackingPointResponseDto?> GetLastPositionByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);
    Task<TrackingPointResponseDto?> GetLastPositionByDeliveryIdAsync(Guid deliveryId, CancellationToken cancellationToken = default);
}
