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

    public async Task<List<Driver>> GetPendingDriversAsync()
    {
        return await _context.Drivers
            .Include(d => d.User)
            .Where(d => d.Status == DriverStatus.PendingVerification)
            .ToListAsync();
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
}






