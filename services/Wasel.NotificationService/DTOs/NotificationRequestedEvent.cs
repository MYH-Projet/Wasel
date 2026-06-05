namespace Wasel.NotificationService.DTOs;

public sealed class NotificationRequestedEvent
{
    public Guid EventId { get; init; }
    public Guid RecipientUserId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string[] Channels { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}
