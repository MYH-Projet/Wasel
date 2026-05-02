using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Documents.Entities;
using Wasel.Api.Modules.Documents.Enums;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Documents.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly WaselDbContext _context;

    public DocumentRepository(WaselDbContext context)
    {
        _context = context;
    }

   
    public async Task<Document?> GetByIdWithDossierAsync(Guid documentId)
    {
        return await _context.Documents
            .Include(d => d.DriverDossier)
                .ThenInclude(dossier => dossier.Driver)
            .FirstOrDefaultAsync(d => d.Id == documentId);
    }

   
    public async Task<bool> AreAllDocumentsApprovedAsync(Guid driverDossierId)
    {
        return await _context.Documents
            .Where(d => d.DriverDossierId == driverDossierId)
            .AllAsync(d => d.Status == DocumentStatus.Approved);
    }

   
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}