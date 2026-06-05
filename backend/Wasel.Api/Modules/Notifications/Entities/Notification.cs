using Wasel.Api.Modules.Notifications.Enums;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Notifications.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public NotificationStatus Status { get; set; } = NotificationStatus.UNREAD;

    public string Channel { get; set; } = string.Empty;

    public string? RelatedEntityType { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public string? FirebaseMessageId { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? ReadAt { get; set; }

    // Navigation
    public User User { get; set; } = default!;
}
