using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Deliveries.Entities;

public class Delivery : BaseEntity
{
    public Guid ClientId { get; set; }
    public Guid? DriverId { get; set; }
    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;
    public DateTime? EstimatedDeliveryTime { get; set; }
}
