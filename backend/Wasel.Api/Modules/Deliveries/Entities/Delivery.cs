using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Deliveries.Entities;

public class Delivery : BaseEntity
{
    public Guid ClientId { get; set; }

    public Guid PickupAddressId { get; set; }

    public Guid DropoffAddressId { get; set; }
    
    public Guid? DriverId { get; set; }
    
    public Guid ParcelId { get; set; }

    public DeliveryStatus Status { get; set; } = DeliveryStatus.CREATED;

    public PaymentMethod PaymentMethod { get; set; }

    public decimal DistanceKm { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "MAD";

    public User Client { get; set; } = default!;

    public Address PickupAddress { get; set; } = default!;

    public Address DropoffAddress { get; set; } = default!;

    public Parcel Parcel { get; set; } = default!;

    public ICollection<DeliveryStatusHistory> StatusHistories { get; set; } = new List<DeliveryStatusHistory>();
}