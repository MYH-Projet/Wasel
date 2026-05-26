using Wasel.Api.Modules.Drivers.Entities;

namespace Wasel.Api.Modules.Drivers.Repositories;

public interface IDriverRepository
{
    Task<List<Driver>> GetPendingDriversAsync();
    Task<Driver?> GetDriverDossierAsync(Guid driverId);
}