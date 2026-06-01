namespace Wasel.Api.Modules.Deliveries.DTOs;

public class ActiveDeliveryResponseDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public Guid? DriverId { get; set; }
    public AddressDto PickupAddress { get; set; } = new();
    public AddressDto DropoffAddress { get; set; } = new();
    public string ParcelSummary { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
