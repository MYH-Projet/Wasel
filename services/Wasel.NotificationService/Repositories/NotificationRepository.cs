using Microsoft.EntityFrameworkCore;
using Wasel.NotificationService.Data;
using Wasel.NotificationService.Entities;

namespace Wasel.NotificationService.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        await _dbContext.Notifications.AddAsync(notification, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _dbContext.Notifications.Update(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<UserDeviceToken>> GetActiveDeviceTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserDeviceTokens
            .Where(t => t.UserId == userId && t.IsActive)
            .ToListAsync(cancellationToken);
    }
}
