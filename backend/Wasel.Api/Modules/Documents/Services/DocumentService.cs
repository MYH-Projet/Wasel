using Wasel.Api.Modules.Documents.DTOs;
using Wasel.Api.Modules.Documents.Enums;
using Wasel.Api.Modules.Documents.Repositories;
using Wasel.Api.Modules.Documents.Entities;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Shared.Exceptions;

namespace Wasel.Api.Modules.Documents.Services;

public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDriverRepository _driverRepository;

    public DocumentService(
        IDocumentRepository documentRepository,
        IUserRepository userRepository,
        IDriverRepository driverRepository)
    {
        _documentRepository = documentRepository;
        _userRepository = userRepository;
        _driverRepository = driverRepository;
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

    public async Task<DriverDocumentResponseDto> AddOrReplaceCurrentDriverDocumentAsync(
        string keycloakId,
        AddDriverDocumentRequestDto request)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId)
            ?? throw new ApiException("User not found", 404);

        var driver = await _driverRepository.GetByUserIdWithDossierAndDocumentsAsync(user.Id)
            ?? throw new ApiException("Driver profile not found", 404);

        if (driver.Dossier == null)
        {
            throw new ApiException("Driver dossier not found", 400);
        }

        var existingDocument = driver.Dossier.Documents
            .FirstOrDefault(d => d.DocumentType == request.DocumentType);

        Document documentToReturn;

        if (existingDocument != null)
        {
            existingDocument.ObjectKey = request.ObjectKey;
            existingDocument.Status = DocumentStatus.Pending;
            existingDocument.RejectionReason = null;
            existingDocument.UploadedAt = DateTime.UtcNow;
            
            documentToReturn = existingDocument;
        }
        else
        {
            var newDocument = new Document
            {
                DriverDossierId = driver.Dossier.Id,
                DocumentType = request.DocumentType,
                ObjectKey = request.ObjectKey,
                Status = DocumentStatus.Pending
            };

            await _documentRepository.AddAsync(newDocument);
            documentToReturn = newDocument;
        }

        await _documentRepository.SaveChangesAsync();

        return new DriverDocumentResponseDto
        {
            Id = documentToReturn.Id,
            DocumentType = documentToReturn.DocumentType.ToString(),
            ObjectKey = documentToReturn.ObjectKey,
            Status = documentToReturn.Status.ToString(),
            RejectionReason = documentToReturn.RejectionReason,
            UploadedAt = documentToReturn.UploadedAt,
            VerifiedAt = documentToReturn.VerifiedAt
        };
    }

    public async Task<IEnumerable<DriverDocumentResponseDto>> GetCurrentDriverDocumentsAsync(string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId)
            ?? throw new ApiException("User not found", 404);

        var driver = await _driverRepository.GetByUserIdWithDossierAndDocumentsAsync(user.Id)
            ?? throw new ApiException("Driver profile not found", 404);

        if (driver.Dossier == null)
        {
            throw new ApiException("Driver dossier not found", 400);
        }

        return driver.Dossier.Documents.Select(d => new DriverDocumentResponseDto
        {
            Id = d.Id,
            DocumentType = d.DocumentType.ToString(),
            ObjectKey = d.ObjectKey,
            Status = d.Status.ToString(),
            RejectionReason = d.RejectionReason,
            UploadedAt = d.UploadedAt,
            VerifiedAt = d.VerifiedAt
        });
    }
}