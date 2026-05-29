namespace Wasel.Api.Modules.Complaints.DTOs;

public class CreateComplaintRequestDto
{
    public Guid DeliveryId { get; set; }
    public string ComplaintType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? RequestedAmount { get; set; }
}