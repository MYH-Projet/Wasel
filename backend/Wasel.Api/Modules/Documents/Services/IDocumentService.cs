using Wasel.Api.Modules.Documents.DTOs;

namespace Wasel.Api.Modules.Documents.Services;

public interface IDocumentService
{
    Task<bool> UpdateDocumentStatusAsync(Guid documentId, string status, RejectDocumentRequestDto? request);

    Task<DriverDocumentResponseDto> AddOrReplaceCurrentDriverDocumentAsync(
        string keycloakId,
        AddDriverDocumentRequestDto request);

    Task<IEnumerable<DriverDocumentResponseDto>> GetCurrentDriverDocumentsAsync(
        string keycloakId);
}
