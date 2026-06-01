using Wasel.Api.Modules.Notifications.Enums;

namespace Wasel.Api.Infrastructure.Firebase;

/// <summary>
/// Abstraction for sending push notifications via Firebase Cloud Messaging (FCM).
/// </summary>
public interface IPushNotificationSender
{
    /// <summary>
    /// Sends a push notification to the specified user.
    /// Implementations should be fire-and-forget safe — callers wrap this in try/catch.
    /// </summary>
    Task SendAsync(Guid userId, string title, string body, NotificationType type, CancellationToken cancellationToken = default);
}
