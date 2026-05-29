using Wasel.Api.Shared.Common;
using Wasel.Api.Modules.Complaints.Enums;
using Wasel.Api.Modules.Deliveries.Entities;

namespace Wasel.Api.Modules.Complaints.Entities;

public class Complaint : BaseEntity
{
    public Guid DeliveryId { get; set; }

    public ComplaintType ComplaintType { get; set; }

    public ComplaintStatus Status { get; set; } = ComplaintStatus.Open;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal? RequestedAmount { get; set; }

    public decimal? ApprovedAmount { get; set; }

    public string? AdminComment { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public ICollection<ComplaintEvidence> Evidences { get; set; } = new List<ComplaintEvidence>();

    public Delivery Delivery { get; set; } = default!;
}













