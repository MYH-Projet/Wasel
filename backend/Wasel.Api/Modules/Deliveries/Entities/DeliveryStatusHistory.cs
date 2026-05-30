using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Deliveries.Entities;

public class DeliveryStatusHistory : BaseEntity
{
    public Guid DeliveryId { get; set; }

    public DeliveryStatus Status { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public string? Note { get; set; }

    public Delivery Delivery { get; set; } = default!;

    public Guid? ChangedByDriverId { get; set; }
}

