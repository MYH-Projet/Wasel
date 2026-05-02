using Wasel.Api.Modules.Drivers.DTOs;
using Wasel.Api.Modules.Documents.DTOs;
using Wasel.Api.Modules.Drivers.Repositories;

namespace Wasel.Api.Modules.Drivers.Services;

public class DriverService : IDriverService
{
    private readonly IDriverRepository _driverRepository;

    public DriverService(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    public async Task<List<DriverSummaryDto>> GetPendingDriversAsync()
    {
        var drivers = await _driverRepository.GetPendingDriversAsync();

        return drivers.Select(driver => new DriverSummaryDto
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
        }).ToList();
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

    
}