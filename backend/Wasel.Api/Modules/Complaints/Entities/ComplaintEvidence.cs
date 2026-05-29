using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Complaints.Entities;

public class ComplaintEvidence : BaseEntity
{
    public Guid ComplaintId { get; set; }

    public string ObjectKey { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public Complaint Complaint { get; set; } = default!;
}