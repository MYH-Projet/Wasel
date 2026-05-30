using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Drivers.Repositories;

public class DriverRepository : IDriverRepository
{
    private readonly WaselDbContext _context;

    public DriverRepository(WaselDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Driver> Items, int TotalCount)> GetPendingDriversAsync(
    int page,
    int pageSize,
    string? search)
    {
        var query = _context.Drivers
            .Include(d => d.User)
            .Where(d => d.Status == DriverStatus.PendingVerification)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";

            query = query.Where(d =>
                EF.Functions.ILike(d.User.FirstName, pattern) ||
                EF.Functions.ILike(d.User.LastName, pattern) ||
                EF.Functions.ILike(d.User.Email, pattern) ||
                EF.Functions.ILike(d.User.Phone, pattern) ||
                EF.Functions.ILike(d.User.Cin, pattern));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Driver?> GetDriverDossierAsync(Guid driverId)
    {
        return await _context.Drivers
            .Include(d => d.User)
            .Include(d => d.Dossier)
                .ThenInclude(dossier => dossier!.Documents)
            .FirstOrDefaultAsync(d => d.Id == driverId);
    }

    public async Task<Driver?> GetByUserIdAsync(Guid userId)
    {
        return await _context.Drivers
            .FirstOrDefaultAsync(d => d.UserId == userId);
    }

    public async Task<Driver?> GetByIdAsync(Guid id)
    {
        return await _context.Drivers
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<(List<Driver> Items, int TotalCount)> GetAdminDriversAsync(
    int page,
    int pageSize,
    string? search,
    List<DriverStatus>? driverStatuses,
    List<DriverDossierStatus>? dossierStatuses,
    DateTime? startDate,
    DateTime? endDate)
    {
        var query = _context.Drivers
            .Include(d => d.User)
            .Include(d => d.Dossier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";

            query = query.Where(d =>
                EF.Functions.ILike(d.User.FirstName, pattern) ||
                EF.Functions.ILike(d.User.LastName, pattern) ||
                EF.Functions.ILike(d.User.Email, pattern) ||
                EF.Functions.ILike(d.User.Phone, pattern) ||
                EF.Functions.ILike(d.User.Cin, pattern));
        }

        if (driverStatuses is not null && driverStatuses.Any())
        {
            query = query.Where(d => driverStatuses.Contains(d.Status));
        }

        if (dossierStatuses is not null && dossierStatuses.Any())
        {
            query = query.Where(d =>
                d.Dossier != null &&
                dossierStatuses.Contains(d.Dossier.Status));
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

    public async Task<Driver?> GetByUserIdWithDossierAndVehicleAsync(Guid userId)
    {
        return await _context.Drivers
            .Include(d => d.Dossier)
            .Include(d => d.Vehicle)
            .FirstOrDefaultAsync(d => d.UserId == userId);
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId)
    {
        return await _context.Drivers.AnyAsync(d => d.UserId == userId);
    }

    public async Task<bool> ExistsByIdAsync(Guid driverId)
    {
        return await _context.Drivers.AnyAsync(d => d.Id == driverId);
    }

    public async Task<bool> ExistsByPermitNumberAsync(string permitNumber)
    {
        return await _context.Drivers.AnyAsync(d => d.PermitNumber == permitNumber);
    }

    public async Task AddAsync(Driver driver)
    {
        await _context.Drivers.AddAsync(driver);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}






