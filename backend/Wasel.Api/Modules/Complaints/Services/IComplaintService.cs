using Wasel.Api.Modules.Complaints.DTOs;

namespace Wasel.Api.Modules.Complaints.Services;

public interface IComplaintService
{
    Task<object> CreateComplaintAsync(CreateComplaintRequestDto request, string keycloakId);
    Task<object> AddEvidenceAsync(Guid complaintId, AddComplaintEvidenceRequestDto request, string keycloakId);
    Task<object> GetMyComplaintsAsync(string keycloakId, string? status, int page, int pageSize);
    Task<object> GetComplaintDetailsAsync(Guid complaintId, string keycloakId);
    Task<object> GetAdminComplaintsAsync(
    string? status,
    string? type,
    DateTime? fromDate,
    DateTime? toDate,
    int page,
    int pageSize);

    Task<object> GetAdminComplaintDetailsAsync(Guid complaintId);

    Task<object> UpdateComplaintStatusAsync(Guid complaintId, UpdateComplaintStatusRequestDto request);

    Task<object> ResolveComplaintAsync(Guid complaintId, ResolveComplaintRequestDto request);
}