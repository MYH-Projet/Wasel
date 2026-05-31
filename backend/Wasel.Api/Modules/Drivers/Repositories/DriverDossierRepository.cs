using Microsoft.EntityFrameworkCore;
using Wasel.Api.Shared.Database;
using Wasel.Api.Modules.Drivers.Entities;

namespace Wasel.Api.Modules.Drivers.Repositories;

public class DriverDossierRepository : IDriverDossierRepository
{
    private readonly WaselDbContext _context;

    public DriverDossierRepository(WaselDbContext context)
    {
        _context = context;
    }

    public async Task<DriverDossier?> GetByIdAsync(Guid dossierId)
    {
        return await _context.DriverDossiers
            .FirstOrDefaultAsync(d => d.Id == dossierId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}