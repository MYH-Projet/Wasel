using Wasel.Api.Modules.Complaints.Entities;
using Wasel.Api.Modules.Complaints.Enums;

namespace Wasel.Api.Modules.Complaints.Repositories;

public interface IComplaintRepository
{
    Task<bool> ExistsByDeliveryIdAsync(Guid deliveryId);
    Task AddAsync(Complaint complaint);
    Task<Complaint?> GetByIdWithDeliveryAsync(Guid complaintId);
   Task<int> CountEvidencesAsync(Guid complaintId);
   Task AddEvidenceAsync(ComplaintEvidence evidence);
   Task<List<Complaint>> GetMyComplaintsAsync(
    Guid clientId,
    ComplaintStatus? status,
    int page,
    int pageSize);
    Task<int> CountMyComplaintsAsync(Guid clientId, ComplaintStatus? status);
    Task<Complaint?> GetDetailsByIdAsync(Guid complaintId);
    Task<List<Complaint>> GetAdminComplaintsAsync(
    ComplaintStatus? status,
    ComplaintType? type,
    DateTime? fromDate,
    DateTime? toDate,
    int page,
    int pageSize);

    Task<int> CountAdminComplaintsAsync(
        ComplaintStatus? status,
        ComplaintType? type,
        DateTime? fromDate,
        DateTime? toDate);

    Task<Complaint?> GetAdminDetailsByIdAsync(Guid complaintId);

    Task SaveChangesAsync();
}