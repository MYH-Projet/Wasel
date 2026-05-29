using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Drivers.Enums;
namespace Wasel.Api.Modules.Drivers.Repositories;

public interface IDriverRepository
{
    Task<(List<Driver> Items, int TotalCount)> GetPendingDriversAsync(
    int page,
    int pageSize,
    string? search);
    Task<Driver?> GetDriverDossierAsync(Guid driverId);
    Task<Driver?> GetByUserIdAsync(Guid userId);
    Task<(List<Driver> Items, int TotalCount)> GetAdminDriversAsync(
    int page,
    int pageSize,
    string? search,
    List<DriverStatus>? driverStatuses,
    List<DriverDossierStatus>? dossierStatuses,
    DateTime? startDate,
    DateTime? endDate);
}