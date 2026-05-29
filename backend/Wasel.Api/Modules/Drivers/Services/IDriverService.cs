using Wasel.Api.Modules.Drivers.DTOs;
using Wasel.Api.Modules.Deliveries.DTOs;

namespace Wasel.Api.Modules.Drivers.Services;

public interface IDriverService
{
    Task<PagedResultDto<DriverSummaryDto>> GetPendingDriversAsync(
    int page,
    int pageSize,
    string? search);
    Task<DriverDossierDto?> GetDriverDossierAsync(Guid driverId);
    Task<PagedResultDto<AdminDriverListItemDto>> GetAdminDriversAsync(
    int page,
    int pageSize,
    string? search,
    string? driverStatus,
    string? dossierStatus,
    DateTime? startDate,
    DateTime? endDate);
}