using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Tracking.Entities;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Tracking.Repositories;

public class TrackingRepository : ITrackingRepository
{
    private readonly WaselDbContext _context;

    public TrackingRepository(WaselDbContext context)
    {
        _context = context;
    }

    public async Task<TrackingPoint?> GetLastPositionByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default)
    {
        return await _context.TrackingPoints
            .Where(p => p.DriverId == driverId)
            .OrderByDescending(p => p.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TrackingPoint?> GetLastPositionByDeliveryIdAsync(Guid deliveryId, CancellationToken cancellationToken = default)
    {
        return await _context.TrackingPoints
            .Where(p => p.DeliveryId == deliveryId)
            .OrderByDescending(p => p.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TrackingPoint> AddTrackingPointAsync(TrackingPoint point, CancellationToken cancellationToken = default)
    {
        _context.TrackingPoints.Add(point);
        await _context.SaveChangesAsync(cancellationToken);
        return point;
    }

    public async Task<Wasel.Api.Modules.Drivers.Entities.Driver?> GetDriverByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Drivers.FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
    }

    public async Task<Wasel.Api.Modules.Deliveries.Entities.Delivery?> GetDeliveryByIdAsync(Guid deliveryId, CancellationToken cancellationToken = default)
    {
        return await _context.Deliveries.FirstOrDefaultAsync(d => d.Id == deliveryId, cancellationToken);
    }
}
