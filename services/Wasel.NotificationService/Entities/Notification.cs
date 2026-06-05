using Wasel.NotificationService.Enums;

namespace Wasel.NotificationService.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public NotificationStatus Status { get; set; } = NotificationStatus.CREATED;

    public string Channel { get; set; } = string.Empty;

    public string? RelatedEntityType { get; set; }

    public Guid? RelatedEntityId { get; set; }

    public string? FirebaseMessageId { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SentAt { get; set; }

    public DateTime? ReadAt { get; set; }
}
