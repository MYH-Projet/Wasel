namespace Wasel.Api.Modules.Notifications.DTOs;

public class NotificationsPageResponseDto
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int UnreadCount { get; set; }
    public IEnumerable<NotificationResponseDto> Items { get; set; } = Enumerable.Empty<NotificationResponseDto>();
}
