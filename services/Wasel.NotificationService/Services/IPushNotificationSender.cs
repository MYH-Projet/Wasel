namespace Wasel.NotificationService.Services;

public interface IPushNotificationSender
{
    Task<PushSendResult> SendAsync(
        string deviceToken,
        string title,
        string message,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default);
}
