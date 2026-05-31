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

    public async Task<Document?> GetByIdAsync(Guid documentId)
    {
        return await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == documentId);
    }

    public async Task<bool> AreAllDocumentsApprovedAsync(Guid driverDossierId)
    {
        return await _context.Documents
            .Where(d => d.DriverDossierId == driverDossierId)
            .AllAsync(d => d.Status == DocumentStatus.Approved);
    }

    public async Task<Document?> GetByDossierIdAndTypeAsync(Guid driverDossierId, DocumentType type)
    {
        return await _context.Documents
            .FirstOrDefaultAsync(d => d.DriverDossierId == driverDossierId && d.DocumentType == type);
    }

    public async Task<IEnumerable<Document>> GetByDossierIdAsync(Guid driverDossierId)
    {
        return await _context.Documents
            .Where(d => d.DriverDossierId == driverDossierId)
            .ToListAsync();
    }

    public async Task AddAsync(Document document)
    {
        await _context.Documents.AddAsync(document);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}