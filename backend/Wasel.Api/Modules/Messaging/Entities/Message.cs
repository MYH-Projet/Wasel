using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Messaging.Entities;

public class Message : BaseEntity
{
    public Guid SenderId { get; set; }

    public Guid DeliveryId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    public User Sender { get; set; } = default!;

    public Delivery Delivery { get; set; } = default!;
}