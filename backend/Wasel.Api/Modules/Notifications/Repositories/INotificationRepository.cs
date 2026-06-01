using Wasel.Api.Modules.Notifications.Entities;

namespace Wasel.Api.Modules.Notifications.Repositories;

public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<List<Notification>> GetByUserIdPagedAsync(Guid userId, int page, int pageSize);
    Task<int> CountByUserIdAsync(Guid userId);
    Task<int> CountUnreadByUserIdAsync(Guid userId);
    Task<Notification?> GetByIdAsync(Guid id);
    Task<int> MarkAllReadAsync(Guid userId);
    Task SaveChangesAsync();
}
