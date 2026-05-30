using Wasel.Api.Modules.Messaging.DTOs;

namespace Wasel.Api.Modules.Messaging.Services;

public interface IMessagingService
{
    Task<object> SendMessageAsync(SendMessageRequestDto request, string keycloakId);
    Task<List<MessageHistoryItemDto>> GetMessageHistoryAsync(Guid deliveryId, string keycloakId);
}