using Microsoft.Extensions.Logging;

namespace Wasel.NotificationService.Services;

public class NoopEmailSender : IEmailSender
{
    private readonly ILogger<NoopEmailSender> _logger;

    public NoopEmailSender(ILogger<NoopEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("NOOP EMAIL: Would send email to {To}. Subject: '{Subject}'", to, subject);
        return Task.CompletedTask;
    }
}
