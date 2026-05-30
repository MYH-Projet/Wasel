using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Shared.Database;
using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Drivers.Enums;


namespace Wasel.Api.Modules.Deliveries.Repositories;

public class DeliveryRepository : IDeliveryRepository
{
    private readonly WaselDbContext _context;

    public DeliveryRepository(WaselDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByKeycloakIdAsync(string keycloakId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);
    }

    public async Task<bool> UserHasClientActiveModeAsync(Guid userId)
    {
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);

        return preference is not null
            && preference.ActiveAppMode == ActiveAppMode.CLIENT;
    }

    public async Task<Delivery> CreateDeliveryRequestAsync(
        Address pickupAddress,
        Address dropoffAddress,
        Parcel parcel,
        Delivery delivery,
        List<DeliveryStatusHistory> histories)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Addresses.Add(pickupAddress);
            _context.Addresses.Add(dropoffAddress);

            _context.Parcels.Add(parcel);

            delivery.PickupAddress = pickupAddress;
            delivery.DropoffAddress = dropoffAddress;
            delivery.Parcel = parcel;

            _context.Deliveries.Add(delivery);

            foreach (var history in histories)
            {
                history.Delivery = delivery;
                _context.DeliveryStatusHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return delivery;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UserHasDriverActiveModeAsync(Guid userId)
    {
        var preference = await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);

        return preference is not null
            && preference.ActiveAppMode == ActiveAppMode.DRIVER;
    }

    public async Task<bool> UserHasApprovedDriverAsync(Guid userId)
    {
        var driver = await _context.Drivers
            .FirstOrDefaultAsync(d => d.UserId == userId);

        return driver is not null
            && driver.Status == DriverStatus.Approved;
    }

    public IQueryable<Delivery> GetAvailableDeliveriesQuery()
    {
        return _context.Deliveries
            .Include(d => d.PickupAddress)
            .Include(d => d.DropoffAddress)
            .Include(d => d.Parcel)
            .Where(d => d.Status == DeliveryStatus.WAITING_DRIVER)
            .Where(d =>
                d.PickupAddress.Latitude != null &&
                d.PickupAddress.Longitude != null);
    }

    public async Task<Delivery?> GetByIdAsync(Guid id) =>
        await _context.Deliveries
            .Include(d => d.StatusHistories)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task UpdateAsync(Delivery delivery)
    {
            _context.Deliveries.Update(delivery);
            await _context.SaveChangesAsync();
    }

    public async Task AddStatusHistoryAsync(DeliveryStatusHistory history)
    {
            _context.DeliveryStatusHistories.Add(history);
            await _context.SaveChangesAsync();
    }

    public async Task CancelDeliveryAsync(Delivery delivery, DeliveryStatus newStatus, string? reason)
        {
            delivery.Status = newStatus;

            delivery.DriverId = null;

            _context.Deliveries.Update(delivery);
            await _context.SaveChangesAsync();

            var history = new DeliveryStatusHistory
            {
                DeliveryId = delivery.Id,
                Status = newStatus,
                Note = reason,
                CreatedAt = DateTime.UtcNow
            };
            await AddStatusHistoryAsync(history);
        }

    public async Task<Delivery?> GetDeliveryWithDetailsAsync(Guid deliveryId)
    {
        return await _context.Deliveries
            .Include(d => d.PickupAddress)
            .Include(d => d.DropoffAddress)  
            .Include(d => d.Parcel)
            .Include(d => d.StatusHistories)
            .FirstOrDefaultAsync(d => d.Id == deliveryId);
    }

    public async Task<List<Delivery>> GetDriverMissionsAsync(Guid driverId, int page, int pageSize)
    {
        return await _context.Deliveries
            .Include(d => d.PickupAddress)
            .Include(d => d.DropoffAddress)
            .Where(d => d.DriverId == driverId)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountDriverMissionsAsync(Guid driverId)
    {
        return await _context.Deliveries
            .Where(d => d.DriverId == driverId)
            .CountAsync();
    }

    public async Task<List<Delivery>> GetClientDeliveriesAsync(Guid clientId, int page, int pageSize)
    {
        return await _context.Deliveries
            .Include(d => d.PickupAddress)
            .Include(d => d.DropoffAddress)
            .Where(d => d.ClientId == clientId)
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountClientDeliveriesAsync(Guid clientId)
    {
        return await _context.Deliveries
            .Where(d => d.ClientId == clientId)
            .CountAsync();
    }

    public async Task<(List<Delivery> Items, int TotalCount)> GetAdminDeliveriesAsync(
    int page,
    int pageSize,
    string? search,
    List<DeliveryStatus>? statuses,
    DateTime? startDate,
    DateTime? endDate)
    {
        var query = _context.Deliveries
            .Include(d => d.Client)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";

            query = query.Where(d =>
                EF.Functions.ILike(d.Id.ToString(), pattern) ||
                EF.Functions.ILike(d.Client.FirstName, pattern) ||
                EF.Functions.ILike(d.Client.LastName, pattern) ||
                EF.Functions.ILike(d.Client.Email, pattern));
        }

        if (statuses is not null && statuses.Any())
        {
            query = query.Where(d => statuses.Contains(d.Status));
        }

        if (startDate.HasValue)
        {
            var utcStartDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
            query = query.Where(d => d.CreatedAt >= utcStartDate);
        }

        if (endDate.HasValue)
        {
            var utcEndDate = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
            query = query.Where(d => d.CreatedAt <= utcEndDate);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    
}