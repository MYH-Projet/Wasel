using Wasel.Api.Modules.Documents.Entities;
using Wasel.Api.Modules.Documents.Enums;

namespace Wasel.Api.Modules.Documents.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdAsync(Guid documentId);

    Task<bool> AreAllDocumentsApprovedAsync(Guid driverDossierId);

    Task<Document?> GetByDossierIdAndTypeAsync(Guid driverDossierId, DocumentType type);
    
    Task<IEnumerable<Document>> GetByDossierIdAsync(Guid driverDossierId);

    Task AddAsync(Document document);

    Task SaveChangesAsync();
}