namespace Wasel.Api.Modules.Complaints.DTOs;

public class ComplaintDetailsDto
{
    public Guid Id { get; set; }
    public Guid DeliveryId { get; set; }
    public string ComplaintType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? AdminComment { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<ComplaintEvidenceDto> Evidences { get; set; } = new();
}