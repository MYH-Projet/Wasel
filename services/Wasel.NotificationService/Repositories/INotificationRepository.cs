using Wasel.NotificationService.Entities;

namespace Wasel.NotificationService.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default);

    Task<List<UserDeviceToken>> GetActiveDeviceTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
