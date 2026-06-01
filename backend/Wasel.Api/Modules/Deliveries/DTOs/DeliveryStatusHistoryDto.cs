namespace Wasel.Api.Modules.Deliveries.DTOs;

public class DeliveryStatusHistoryDto
{
    public Guid Id { get; set; }
    public string DeliveryStatus { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}