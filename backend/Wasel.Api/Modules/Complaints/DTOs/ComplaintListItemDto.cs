namespace Wasel.Api.Modules.Complaints.DTOs;

public class ComplaintListItemDto
{
    public Guid Id { get; set; }
    public Guid DeliveryId { get; set; }
    public string ComplaintType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal? RequestedAmount { get; set; }
}