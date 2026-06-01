namespace Wasel.Api.Modules.Deliveries.DTOs;

public class AvailableDeliveryDto
{
    public Guid Id { get; set; }

    public AddressDto PickupAddress { get; set; } = default!;

    public AddressDto DropoffAddress { get; set; } = default!;

    public double EstimatedDistanceKm { get; set; }

    public decimal Price { get; set; }

    public decimal Weight { get; set; }

    public bool IsFragile { get; set; }
}