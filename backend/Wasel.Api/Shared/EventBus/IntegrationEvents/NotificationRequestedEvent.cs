namespace Wasel.Api.Shared.EventBus.IntegrationEvents;

public sealed class NotificationRequestedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public Guid RecipientUserId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string? RelatedEntityType { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string[] Channels { get; init; } = ["IN_APP", "PUSH"];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}