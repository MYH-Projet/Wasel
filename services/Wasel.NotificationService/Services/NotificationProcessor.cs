using Microsoft.Extensions.Logging;
using Wasel.NotificationService.DTOs;
using Wasel.NotificationService.Entities;
using Wasel.NotificationService.Enums;
using Wasel.NotificationService.Repositories;

namespace Wasel.NotificationService.Services;

public class NotificationProcessor : INotificationProcessor
{
    private readonly INotificationRepository _repository;
    private readonly IPushNotificationSender _pushSender;
    private readonly ILogger<NotificationProcessor> _logger;

    public NotificationProcessor(
        INotificationRepository repository,
        IPushNotificationSender pushSender,
        ILogger<NotificationProcessor> logger)
    {
        _repository = repository;
        _pushSender = pushSender;
        _logger = logger;
    }

    public async Task ProcessAsync(NotificationRequestedEvent requestedEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Processing NotificationEvent {EventId} for User {UserId}", 
            requestedEvent.EventId, requestedEvent.RecipientUserId);

        if (!Enum.TryParse<NotificationType>(requestedEvent.Type, true, out var type))
        {
            _logger.LogWarning("Unknown NotificationType '{Type}'. Defaulting to NEW_MESSAGE.", requestedEvent.Type);
            type = NotificationType.NEW_MESSAGE;
        }

        var mainChannel = requestedEvent.Channels.FirstOrDefault() ?? NotificationChannel.IN_APP.ToString();

        var notification = new Notification
        {
            UserId = requestedEvent.RecipientUserId,
            Type = type,
            Title = requestedEvent.Title,
            Body = requestedEvent.Message,
            Channel = mainChannel,
            RelatedEntityType = requestedEvent.RelatedEntityType,
            RelatedEntityId = requestedEvent.RelatedEntityId,
            Status = NotificationStatus.CREATED
        };

        // 1. Save initial notification
        await _repository.AddAsync(notification, cancellationToken);

        // 2. Determine if Push is needed
        if (requestedEvent.Channels.Contains(NotificationChannel.PUSH.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            var tokens = await _repository.GetActiveDeviceTokensAsync(requestedEvent.RecipientUserId, cancellationToken);
            if (!tokens.Any())
            {
                _logger.LogInformation("No active device tokens found for User {UserId}", requestedEvent.RecipientUserId);
                notification.Status = NotificationStatus.NO_DEVICE_TOKEN;
            }
            else
            {
                // Send to all active tokens (or just the most recent one). We'll send to all.
                bool anySuccess = false;
                string? lastError = null;
                string? messageIds = "";

                foreach (var token in tokens)
                {
                    var result = await _pushSender.SendAsync(
                        token.Token,
                        requestedEvent.Title,
                        requestedEvent.Message,
                        null,
                        cancellationToken);

                    if (result.Success)
                    {
                        anySuccess = true;
                        messageIds += result.MessageId + ",";
                    }
                    else
                    {
                        lastError = result.ErrorMessage;
                    }
                }

                if (anySuccess)
                {
                    notification.Status = NotificationStatus.SENT;
                    notification.SentAt = DateTime.UtcNow;
                    notification.FirebaseMessageId = messageIds.TrimEnd(',');
                }
                else
                {
                    notification.Status = NotificationStatus.FAILED;
                    notification.ErrorMessage = lastError;
                }
            }
        }

        // 3. Update notification status
        await _repository.UpdateAsync(notification, cancellationToken);
        
        _logger.LogInformation("Finished processing NotificationEvent {EventId}. Final Status: {Status}", 
            requestedEvent.EventId, notification.Status);
    }
}
