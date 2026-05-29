using Wasel.Api.Modules.Drivers.DTOs;
using Wasel.Api.Modules.Documents.DTOs;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Deliveries.DTOs;
using Wasel.Api.Modules.Drivers.Enums;
namespace Wasel.Api.Modules.Drivers.Services;

public class DriverService : IDriverService
{
    private readonly IDriverRepository _driverRepository;

    public DriverService(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    public async Task<PagedResultDto<DriverSummaryDto>> GetPendingDriversAsync(
    int page,
    int pageSize,
    string? search)
{
    if (page <= 0)
        page = 1;

    if (pageSize <= 0)
        pageSize = 10;

    var result = await _driverRepository.GetPendingDriversAsync(
        page,
        pageSize,
        search);

    return new PagedResultDto<DriverSummaryDto>
    {
        Page = page,
        PageSize = pageSize,
        TotalItems = result.TotalCount,
        Items = result.Items.Select(driver => new DriverSummaryDto
        {
            DriverId = driver.Id,
            UserId = driver.UserId,
            FirstName = driver.User.FirstName,
            LastName = driver.User.LastName,
            Email = driver.User.Email,
            Phone = driver.User.Phone,
            PermitNumber = driver.PermitNumber,
            Status = driver.Status,
            CreatedAt = driver.CreatedAt
        }).ToList()
    };
}

    public async Task<DriverDossierDto?> GetDriverDossierAsync(Guid driverId)
    {
        var driver = await _driverRepository.GetDriverDossierAsync(driverId);

        if (driver is null)
        {
            return null;
        }

        return new DriverDossierDto
        {
            DriverId = driver.Id,
            UserId = driver.UserId,

            FirstName = driver.User.FirstName,
            LastName = driver.User.LastName,
            Email = driver.User.Email,
            Phone = driver.User.Phone,
            Cin = driver.User.Cin,

            PermitNumber = driver.PermitNumber,
            DriverStatus = driver.Status,

            DossierId = driver.Dossier?.Id,
            DossierStatus = driver.Dossier?.Status,
            SubmissionDate = driver.Dossier?.SubmissionDate,
            VerificationDate = driver.Dossier?.VerificationDate,
            RejectionReason = driver.Dossier?.RejectionReason,

            Documents = driver.Dossier?.Documents.Select(document => new DocumentResponseDto
            {
                DocumentId = document.Id,
                DocumentType = document.DocumentType,
                ObjectKey = document.ObjectKey,
                Status = document.Status,
                RejectionReason = document.RejectionReason,
                UploadedAt = document.UploadedAt,
                VerifiedAt = document.VerifiedAt
            }).ToList() ?? new List<DocumentResponseDto>()
        };
    }
    
    public async Task<PagedResultDto<AdminDriverListItemDto>> GetAdminDriversAsync(
    int page,
    int pageSize,
    string? search,
    string? driverStatus,
    string? dossierStatus,
    DateTime? startDate,
    DateTime? endDate)
    {
        if (page <= 0)
            page = 1;

        if (pageSize <= 0)
            pageSize = 10;

        List<DriverStatus>? parsedDriverStatuses = null;

        if (!string.IsNullOrWhiteSpace(driverStatus))
        {
            parsedDriverStatuses = new List<DriverStatus>();

            var values = driverStatus.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var value in values)
            {
                if (!Enum.TryParse<DriverStatus>(value.Trim(), true, out var parsed))
                    throw new InvalidOperationException($"Invalid driver status: {value}");

                parsedDriverStatuses.Add(parsed);
            }
        }

        List<DriverDossierStatus>? parsedDossierStatuses = null;

        if (!string.IsNullOrWhiteSpace(dossierStatus))
        {
            parsedDossierStatuses = new List<DriverDossierStatus>();

            var values = dossierStatus.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var value in values)
            {
                if (!Enum.TryParse<DriverDossierStatus>(value.Trim(), true, out var parsed))
                    throw new InvalidOperationException($"Invalid dossier status: {value}");

                parsedDossierStatuses.Add(parsed);
            }
        }

        var result = await _driverRepository.GetAdminDriversAsync(
            page,
            pageSize,
            search,
            parsedDriverStatuses,
            parsedDossierStatuses,
            startDate,
            endDate);

        return new PagedResultDto<AdminDriverListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = result.TotalCount,
            Items = result.Items.Select(driver => new AdminDriverListItemDto
            {
                DriverId = driver.Id,
                UserId = driver.UserId,
                FirstName = driver.User.FirstName,
                LastName = driver.User.LastName,
                Email = driver.User.Email,
                Phone = driver.User.Phone,
                Cin = driver.User.Cin,
                PermitNumber = driver.PermitNumber,
                DriverStatus = driver.Status.ToString(),
                DossierStatus = driver.Dossier?.Status.ToString(),
                CreatedAt = driver.CreatedAt
            }).ToList()
        };
    }
    
}