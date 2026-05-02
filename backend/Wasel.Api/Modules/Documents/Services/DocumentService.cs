using Wasel.Api.Modules.Documents.DTOs;
using Wasel.Api.Modules.Documents.Enums;
using Wasel.Api.Modules.Documents.Repositories;
using Wasel.Api.Modules.Drivers.Enums;

namespace Wasel.Api.Modules.Documents.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<bool> UpdateDocumentStatusAsync(Guid documentId, string status, RejectDocumentRequestDto? request)
    {
        var document = await _documentRepository.GetByIdWithDossierAsync(documentId);

        if (document is null)
        {
            return false;
        }

        var normalizedStatus = status.ToLower();

        if (normalizedStatus == "approved")
        {
            document.Status = DocumentStatus.Approved;
            document.VerifiedAt = DateTime.UtcNow;
            document.RejectionReason = null;

            var allApproved = await _documentRepository.AreAllDocumentsApprovedAsync(document.DriverDossierId);

            if (allApproved)
            {
                document.DriverDossier.Status = DriverDossierStatus.Approved;
                document.DriverDossier.VerificationDate = DateTime.UtcNow;
                document.DriverDossier.RejectionReason = null;

                document.DriverDossier.Driver.Status = DriverStatus.Approved;
            }
        }
        else if (normalizedStatus == "rejected")
        {
            if (request is null || string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                throw new ArgumentException("Rejection reason is required");
            }

            document.Status = DocumentStatus.Rejected;
            document.VerifiedAt = DateTime.UtcNow;
            document.RejectionReason = request.RejectionReason;

            document.DriverDossier.Status = DriverDossierStatus.Rejected;
            document.DriverDossier.VerificationDate = DateTime.UtcNow;
            document.DriverDossier.RejectionReason = request.RejectionReason;

            document.DriverDossier.Driver.Status = DriverStatus.Rejected;
        }
        else
        {
            throw new ArgumentException("Invalid status. Use approved or rejected.");
        }

        await _documentRepository.SaveChangesAsync();

        return true;
    }
}










