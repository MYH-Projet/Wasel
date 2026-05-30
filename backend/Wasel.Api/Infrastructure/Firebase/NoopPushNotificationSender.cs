using Wasel.Api.Modules.Notifications.Enums;

namespace Wasel.Api.Infrastructure.Firebase;

/// <summary>
/// No-op implementation of IPushNotificationSender.
/// Used when Firebase credentials are not configured.
/// Logs the push attempt and returns immediately.
/// </summary>
public class NoopPushNotificationSender : IPushNotificationSender
{
    private readonly ILogger<NoopPushNotificationSender> _logger;

    public NoopPushNotificationSender(ILogger<NoopPushNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(Guid userId, string title, string body, NotificationType type, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[FCM Noop] Push skipped — UserId={UserId}, Type={Type}, Title={Title}",
            userId, type, title);

        return Task.CompletedTask;
    }
}
