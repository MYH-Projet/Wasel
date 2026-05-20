using Wasel.Api.Modules.Drivers.DTOs;

namespace Wasel.Api.Modules.Drivers.Services;

public interface IDriverDossierService
{
    Task<bool> UpdateDossierStatusAsync(
        Guid dossierId,
        UpdateDriverDossierStatusRequestDto request
    );
}