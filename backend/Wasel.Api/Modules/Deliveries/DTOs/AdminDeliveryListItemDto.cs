namespace Wasel.Api.Modules.Deliveries.DTOs;

public class AdminDeliveryListItemDto
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public Guid? DriverId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}