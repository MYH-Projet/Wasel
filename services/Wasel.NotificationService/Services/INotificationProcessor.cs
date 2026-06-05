using Wasel.NotificationService.DTOs;

namespace Wasel.NotificationService.Services;

public interface INotificationProcessor
{
    Task ProcessAsync(NotificationRequestedEvent requestedEvent, CancellationToken cancellationToken = default);
}
