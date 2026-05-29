using Wasel.Api.Modules.Messaging.Entities;
using Wasel.Api.Shared.Database;
using Microsoft.EntityFrameworkCore;
namespace Wasel.Api.Modules.Messaging.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly WaselDbContext _db;

    public MessageRepository(WaselDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Message message)
    {
        _db.Messages.Add(message);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Message>> GetByDeliveryIdAsync(Guid deliveryId)
    {
        return await _db.Messages
            .Where(m => m.DeliveryId == deliveryId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }
}