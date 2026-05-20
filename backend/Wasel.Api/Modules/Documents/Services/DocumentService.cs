using Wasel.Api.Modules.Documents.DTOs;
using Wasel.Api.Modules.Documents.Enums;
using Wasel.Api.Modules.Documents.Repositories;

namespace Wasel.Api.Modules.Documents.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;

    public DocumentService(IDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<bool> UpdateDocumentStatusAsync(
        Guid documentId,
        string status,
        RejectDocumentRequestDto? request)
    {
        var document = await _documentRepository.GetByIdAsync(documentId);

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
        }
        else
        {
            throw new ArgumentException("Invalid status. Use approved or rejected.");
        }

        await _documentRepository.SaveChangesAsync();

        return true;
    }
}