using Microsoft.Extensions.Logging;

namespace Wasel.NotificationService.Services;

public class NoopPushNotificationSender : IPushNotificationSender
{
    private readonly ILogger<NoopPushNotificationSender> _logger;

    public NoopPushNotificationSender(ILogger<NoopPushNotificationSender> logger)
    {
        _logger = logger;
    }

    public Task<PushSendResult> SendAsync(
        string deviceToken,
        string title,
        string message,
        Dictionary<string, string>? data = null,
        CancellationToken cancellationToken = default)
    {
        var maskedToken = deviceToken.Length > 10 ? $"{deviceToken[..5]}...{deviceToken[^5..]}" : "***";
        
        _logger.LogInformation(
            "NOOP PUSH: Would send push to {Token}. Title: '{Title}', Message: '{Message}'",
            maskedToken, title, message);

        return Task.FromResult(new PushSendResult
        {
            Success = true,
            MessageId = $"noop-{Guid.NewGuid()}"
        });
    }
}
