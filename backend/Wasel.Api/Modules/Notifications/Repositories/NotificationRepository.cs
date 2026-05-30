using Microsoft.EntityFrameworkCore;
using Wasel.Api.Modules.Notifications.Entities;
using Wasel.Api.Modules.Notifications.Enums;
using Wasel.Api.Shared.Database;

namespace Wasel.Api.Modules.Notifications.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly WaselDbContext _context;

    public NotificationRepository(WaselDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
    }

    public async Task<List<Notification>> GetByUserIdPagedAsync(Guid userId, int page, int pageSize)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> CountByUserIdAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId);
    }

    public async Task<int> CountUnreadByUserIdAsync(Guid userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && n.Status == NotificationStatus.UNREAD);
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _context.Notifications.FindAsync(id);
    }

    public async Task<int> MarkAllReadAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        return await _context.Notifications
            .Where(n => n.UserId == userId && n.Status == NotificationStatus.UNREAD)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Status, NotificationStatus.READ)
                .SetProperty(n => n.ReadAt, now)
                .SetProperty(n => n.UpdatedAt, now));
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
