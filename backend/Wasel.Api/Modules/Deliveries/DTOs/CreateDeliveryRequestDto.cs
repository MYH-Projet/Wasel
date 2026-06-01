using Wasel.Api.Modules.Deliveries.Enums;

namespace Wasel.Api.Modules.Deliveries.DTOs;

public class CreateDeliveryRequestDto
{
    public CreateAddressRequestDto PickupAddress { get; set; } = default!;

    public CreateAddressRequestDto DropoffAddress { get; set; } = default!;

    public CreateParcelRequestDto Parcel { get; set; } = default!;

    public PaymentMethod PaymentMethod { get; set; }
}