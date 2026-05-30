using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Complaints.Entities;
using Wasel.Api.Shared.Database;
using Wasel.Api.Modules.Complaints.Enums;

namespace Wasel.Api.Modules.Complaints.Repositories;

public class ComplaintRepository : IComplaintRepository
{
    private readonly WaselDbContext _db;

    public ComplaintRepository(WaselDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ExistsByDeliveryIdAsync(Guid deliveryId)
    {
        return await _db.Complaints
            .AnyAsync(c => c.DeliveryId == deliveryId);
    }

    public async Task AddAsync(Complaint complaint)
    {
        _db.Complaints.Add(complaint);
        await _db.SaveChangesAsync();
    }

    public async Task<Complaint?> GetByIdWithDeliveryAsync(Guid complaintId)
{
    return await _db.Complaints
        .Include(c => c.Delivery)
        .FirstOrDefaultAsync(c => c.Id == complaintId);
}

public async Task<int> CountEvidencesAsync(Guid complaintId)
{
    return await _db.ComplaintEvidences
        .CountAsync(e => e.ComplaintId == complaintId);
}

public async Task AddEvidenceAsync(ComplaintEvidence evidence)
{
    _db.ComplaintEvidences.Add(evidence);
    await _db.SaveChangesAsync();
}
public async Task<List<Complaint>> GetMyComplaintsAsync(
    Guid clientId,
    ComplaintStatus? status,
    int page,
    int pageSize)
{
    var query = _db.Complaints
        .Include(c => c.Delivery)
        .Where(c => c.Delivery.ClientId == clientId);

    if (status.HasValue)
        query = query.Where(c => c.Status == status.Value);

    return await query
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
}

public async Task<int> CountMyComplaintsAsync(Guid clientId, ComplaintStatus? status)
{
    var query = _db.Complaints
        .Include(c => c.Delivery)
        .Where(c => c.Delivery.ClientId == clientId);

    if (status.HasValue)
        query = query.Where(c => c.Status == status.Value);

    return await query.CountAsync();
}

public async Task<Complaint?> GetDetailsByIdAsync(Guid complaintId)
{
    return await _db.Complaints
        .Include(c => c.Delivery)
        .Include(c => c.Evidences)
        .FirstOrDefaultAsync(c => c.Id == complaintId);
}

public async Task<List<Complaint>> GetAdminComplaintsAsync(
    ComplaintStatus? status,
    ComplaintType? type,
    DateTime? fromDate,
    DateTime? toDate,
    int page,
    int pageSize)
{
    var query = _db.Complaints
        .Include(c => c.Delivery)
        .AsQueryable();

    if (status.HasValue)
        query = query.Where(c => c.Status == status.Value);

    if (type.HasValue)
        query = query.Where(c => c.ComplaintType == type.Value);

    if (fromDate.HasValue)
        query = query.Where(c => c.CreatedAt >= fromDate.Value);

    if (toDate.HasValue)
        query = query.Where(c => c.CreatedAt <= toDate.Value);

    return await query
        .OrderByDescending(c => c.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
}

public async Task<int> CountAdminComplaintsAsync(
    ComplaintStatus? status,
    ComplaintType? type,
    DateTime? fromDate,
    DateTime? toDate)
{
    var query = _db.Complaints.AsQueryable();

    if (status.HasValue)
        query = query.Where(c => c.Status == status.Value);

    if (type.HasValue)
        query = query.Where(c => c.ComplaintType == type.Value);

    if (fromDate.HasValue)
        query = query.Where(c => c.CreatedAt >= fromDate.Value);

    if (toDate.HasValue)
        query = query.Where(c => c.CreatedAt <= toDate.Value);

    return await query.CountAsync();
}

public async Task<Complaint?> GetAdminDetailsByIdAsync(Guid complaintId)
{
    return await _db.Complaints
        .Include(c => c.Delivery)
        .Include(c => c.Evidences)
        .FirstOrDefaultAsync(c => c.Id == complaintId);
}

public async Task SaveChangesAsync()
{
    await _db.SaveChangesAsync();
}
}