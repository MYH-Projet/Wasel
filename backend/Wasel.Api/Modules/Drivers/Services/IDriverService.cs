using Wasel.Api.Modules.Drivers.DTOs;

namespace Wasel.Api.Modules.Drivers.Services;

public interface IDriverService
{
    Task<List<DriverSummaryDto>> GetPendingDriversAsync();
    Task<DriverDossierDto?> GetDriverDossierAsync(Guid driverId);
}