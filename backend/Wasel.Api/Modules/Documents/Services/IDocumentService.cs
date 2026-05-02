using Wasel.Api.Modules.Documents.DTOs;

namespace Wasel.Api.Modules.Documents.Services;

public interface IDocumentService
{
    Task<bool> UpdateDocumentStatusAsync(Guid documentId, string status, RejectDocumentRequestDto? request);
}
