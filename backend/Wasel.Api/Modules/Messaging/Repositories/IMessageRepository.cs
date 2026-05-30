using Wasel.Api.Modules.Messaging.Entities;

namespace Wasel.Api.Modules.Messaging.Repositories;

public interface IMessageRepository
{
    Task AddAsync(Message message);
    Task<List<Message>> GetByDeliveryIdAsync(Guid deliveryId);
}