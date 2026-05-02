using Wasel.Api.Modules.Documents.Entities;

namespace Wasel.Api.Modules.Documents.Repositories;

public interface IDocumentRepository
{
    Task<Document?> GetByIdWithDossierAsync(Guid documentId);

    Task<bool> AreAllDocumentsApprovedAsync(Guid driverDossierId);

    Task SaveChangesAsync();
}



















