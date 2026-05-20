using Wasel.Api.Modules.Drivers.Entities;

namespace Wasel.Api.Modules.Drivers.Repositories;

public interface IDriverDossierRepository
{
    Task<DriverDossier?> GetByIdAsync(Guid dossierId);
    Task SaveChangesAsync();
}