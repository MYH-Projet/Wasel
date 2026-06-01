using Wasel.Api.Modules.Notifications.DTOs;
using Wasel.Api.Modules.Notifications.Enums;

namespace Wasel.Api.Modules.Notifications.Services;

public interface INotificationService
{
    /// <summary>
    /// Creates a notification in DB (UNREAD) then attempts FCM push (non-blocking).
    /// </summary>
    Task CreateAsync(Guid userId, NotificationType type, string title, string body);

    /// <summary>
    /// Returns paginated notifications for the current authenticated user.
    /// </summary>
    Task<NotificationsPageResponseDto> GetMyNotificationsAsync(int page, int pageSize);

    /// <summary>
    /// Marks a single notification as read. Validates ownership.
    /// </summary>
    Task<NotificationResponseDto> MarkAsReadAsync(Guid notificationId);

    /// <summary>
    /// Marks all unread notifications of the current user as read.
    /// </summary>
    Task<MarkAllReadResponseDto> MarkAllAsReadAsync();
}
