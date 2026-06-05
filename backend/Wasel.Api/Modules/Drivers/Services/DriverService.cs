using Wasel.Api.Modules.Drivers.DTOs;
using Wasel.Api.Modules.Documents.DTOs;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Deliveries.DTOs;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Shared.Exceptions;

namespace Wasel.Api.Modules.Drivers.Services;

public class DriverService : IDriverService
{
    private readonly IDriverRepository _driverRepository;
    private readonly IUserRepository _userRepository;

    public DriverService(IDriverRepository driverRepository, IUserRepository userRepository)
    {
        _driverRepository = driverRepository;
        _userRepository = userRepository;
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

        var stats = await _driverRepository.GetDriverStatsAsync(driverId);

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

            TotalDeliveries = stats.TotalDeliveries,
            Rating = stats.Rating,
            CompletionRate = stats.CompletionRate,

            Documents = driver.Dossier?.Documents.Select(document => new DocumentResponseDto
            {
                DocumentId = document.Id,
                DocumentType = document.DocumentType,
                ObjectKey = document.ObjectKey,
                Status = document.Status,
                RejectionReason = document.RejectionReason,
                UploadedAt = document.UploadedAt,
                VerifiedAt = document.VerifiedAt
            }).ToList() ?? new List<DocumentResponseDto>(),

            Vehicle = driver.Vehicle is not null ? new VehicleResponseDto
            {
                Type = driver.Vehicle.Type,
                Matricule = driver.Vehicle.Matricule,
                Marque = driver.Vehicle.Marque,
                Model = driver.Vehicle.Model
            } : null
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

    public async Task<DriverMeResponseDto> RegisterCurrentUserAsDriverAsync(string keycloakId, RegisterDriverRequestDto request)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);
        if (user is null)
        {
            throw new ApiException("User not found", 404);
        }

        if (await _driverRepository.ExistsByUserIdAsync(user.Id))
        {
            throw new ApiException("User already has a driver profile", 409);
        }

        if (await _driverRepository.ExistsByPermitNumberAsync(request.PermitNumber))
        {
            throw new ApiException("Permit number is already in use", 409);
        }

        var driver = new Driver
        {
            UserId = user.Id,
            PermitNumber = request.PermitNumber,
            Status = DriverStatus.PendingVerification,
            Dossier = new DriverDossier
            {
                Status = DriverDossierStatus.Draft
            },
            Vehicle = new Vehicle
            {
                Type = request.Vehicle.Type,
                Matricule = request.Vehicle.Matricule,
                Model = request.Vehicle.Model,
                Marque = request.Vehicle.Marque
            }
        };

        await _driverRepository.AddAsync(driver);
        await _driverRepository.SaveChangesAsync();

        return await GetCurrentDriverProfileAsync(keycloakId);
    }

    public async Task<DriverMeResponseDto> GetCurrentDriverProfileAsync(string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);
        if (user is null)
        {
            throw new ApiException("User not found", 404);
        }

        var driver = await _driverRepository.GetByUserIdWithDossierAndVehicleAsync(user.Id);
        if (driver is null)
        {
            throw new ApiException("Driver profile not found", 404);
        }

        return new DriverMeResponseDto
        {
            DriverId = driver.Id,
            UserId = driver.UserId,
            PermitNumber = driver.PermitNumber,
            DriverStatus = driver.Status,
            DossierStatus = driver.Dossier?.Status ?? DriverDossierStatus.Draft,
            SubmissionDate = driver.Dossier?.SubmissionDate,
            VerificationDate = driver.Dossier?.VerificationDate,
            RejectionReason = driver.Dossier?.RejectionReason,
            CreatedAt = driver.CreatedAt,
            Vehicle = driver.Vehicle is not null ? new VehicleResponseDto
            {
                Type = driver.Vehicle.Type,
                Matricule = driver.Vehicle.Matricule,
                Model = driver.Vehicle.Model,
                Marque = driver.Vehicle.Marque
            } : null
        };
    }

    public async Task<DriverMeResponseDto> SubmitCurrentDriverDossierAsync(string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);
        if (user is null)
        {
            throw new ApiException("User not found", 404);
        }

        var driver = await _driverRepository.GetByUserIdWithDossierAndVehicleAsync(user.Id);
        if (driver is null)
        {
            throw new ApiException("Driver profile not found", 404);
        }

        if (driver.Dossier is null)
        {
            throw new ApiException("Driver dossier not found", 400);
        }

        if (driver.Dossier.Status != DriverDossierStatus.Draft)
        {
            throw new ApiException("Only draft dossiers can be submitted", 400);
        }

        driver.Dossier.Status = DriverDossierStatus.Submitted;
        driver.Dossier.SubmissionDate = DateTime.UtcNow;

        await _driverRepository.SaveChangesAsync();

        // TODO: Notify admin about the new driver submission.

        return await GetCurrentDriverProfileAsync(keycloakId);
    }
}