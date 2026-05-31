namespace Wasel.Api.Modules.Messaging.DTOs;

public class MessageHistoryItemDto
{
    public Guid SenderId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public bool IsRead { get; set; }
}
