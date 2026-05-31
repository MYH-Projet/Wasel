using Wasel.Api.Modules.Drivers.DTOs;
using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Modules.Drivers.Repositories;

namespace Wasel.Api.Modules.Drivers.Services;

public class DriverDossierService : IDriverDossierService
{
    private readonly IDriverDossierRepository _driverDossierRepository;

    public DriverDossierService(IDriverDossierRepository driverDossierRepository)
    {
        _driverDossierRepository = driverDossierRepository;
    }

    public async Task<bool> UpdateDossierStatusAsync(
        Guid dossierId,
        UpdateDriverDossierStatusRequestDto request)
    {
        var dossier = await _driverDossierRepository.GetByIdAsync(dossierId);

        if (dossier is null)
        {
            return false;
        }

        var normalizedStatus = request.Status.ToLower();

        if (normalizedStatus == "approved")
        {
            dossier.Status = DriverDossierStatus.Approved;
            dossier.VerificationDate = DateTime.UtcNow;
            dossier.RejectionReason = null;
        }
        else if (normalizedStatus == "rejected")
        {
            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                throw new ArgumentException("Rejection reason is required");
            }

            dossier.Status = DriverDossierStatus.Rejected;
            dossier.VerificationDate = DateTime.UtcNow;
            dossier.RejectionReason = request.RejectionReason;
        }
        else if (normalizedStatus == "underreview")
        {
            dossier.Status = DriverDossierStatus.UnderReview;
            dossier.VerificationDate = null;
            dossier.RejectionReason = null;
        }
        else if (normalizedStatus == "submitted")
        {
            dossier.Status = DriverDossierStatus.Submitted;
            dossier.VerificationDate = null;
            dossier.RejectionReason = null;
        }
        else if (normalizedStatus == "draft")
        {
            dossier.Status = DriverDossierStatus.Draft;
            dossier.VerificationDate = null;
            dossier.RejectionReason = null;
        }
        else
        {
            throw new ArgumentException(
                "Invalid status. Use draft, submitted, underreview, approved or rejected."
            );
        }

        await _driverDossierRepository.SaveChangesAsync();

        return true;
    }
}