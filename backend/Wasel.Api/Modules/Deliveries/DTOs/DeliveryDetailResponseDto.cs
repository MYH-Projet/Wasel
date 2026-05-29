namespace Wasel.Api.Modules.Deliveries.DTOs;

public class DeliveryDetailResponseDto
{
    public Guid Id { get; set; }
    public string DeliveryStatus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? PickupDate { get; set; }
    public DateTime? CancelledAt { get; set; }
    public DateTime UpdatedAt { get; set; }
 
    public AddressDto PickupAddress { get; set; } = new();
    public AddressDto DeliveryAddress { get; set; } = new();
 
    public ParcelDetailDto Parcel { get; set; } = new();
 
    public PaymentDetailDto Payment { get; set; } = new();
 
    public AssignedDriverDto? AssignedDriver { get; set; }

    public List<DeliveryStatusHistoryDto> StatusHistory { get; set; } = new();
}
 