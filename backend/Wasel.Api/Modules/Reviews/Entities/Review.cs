using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Reviews.Entities;

public class Review : BaseEntity
{
    public Guid ReviewerUserId { get; set; }
    public Guid ReviewedDriverId { get; set; }
    public Guid DeliveryId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }

    public User ReviewerUser { get; set; } = null!;
    public Driver ReviewedDriver { get; set; } = null!;
    public Delivery Delivery { get; set; } = null!;
}
