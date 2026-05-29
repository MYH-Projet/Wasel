using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Deliveries.Repositories;
using Wasel.Api.Modules.Messaging.DTOs;
using Wasel.Api.Modules.Messaging.Entities;
using Wasel.Api.Modules.Messaging.Repositories;
using Wasel.Api.Modules.Users.Repositories;

namespace Wasel.Api.Modules.Messaging.Services;

public class MessagingService : IMessagingService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IUserRepository _userRepository;

    public MessagingService(
        IMessageRepository messageRepository,
        IDeliveryRepository deliveryRepository,
        IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _deliveryRepository = deliveryRepository;
        _userRepository = userRepository;
    }

    public async Task<object> SendMessageAsync(SendMessageRequestDto request, string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
            throw new UnauthorizedAccessException("Utilisateur introuvable.");

        var delivery = await _deliveryRepository.GetByIdAsync(request.DeliveryId);

        if (delivery is null)
            throw new InvalidOperationException("Livraison introuvable.");

        if (delivery.ClientId != user.Id && delivery.DriverId != user.Id)
            throw new UnauthorizedAccessException("Vous n'êtes pas concerné par cette livraison.");

        var activeStatuses = new[]
        {
            DeliveryStatus.ASSIGNED,
            DeliveryStatus.ARRIVED_AT_PICKUP,
            DeliveryStatus.IN_TRANSIT,
            DeliveryStatus.ARRIVED_AT_DROPOFF
        };

        if (!activeStatuses.Contains(delivery.Status))
            throw new InvalidOperationException("La livraison n'est plus active.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new InvalidOperationException("Le contenu du message est obligatoire.");

        var message = new Message
        {
            SenderId = user.Id,
            DeliveryId = request.DeliveryId,
            Content = request.Content,
            SentAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message);

        return new
        {
            message.Id,
            message.SenderId,
            message.DeliveryId,
            message.Content,
            message.SentAt,
            message.ReadAt
        };
    }
    public async Task<List<MessageHistoryItemDto>> GetMessageHistoryAsync(Guid deliveryId, string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
            throw new UnauthorizedAccessException("Utilisateur introuvable.");

        var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);

        if (delivery is null)
            throw new InvalidOperationException("Livraison introuvable.");

        if (delivery.ClientId != user.Id && delivery.DriverId != user.Id)
            throw new UnauthorizedAccessException("Vous n'êtes pas concerné par cette livraison.");

        var messages = await _messageRepository.GetByDeliveryIdAsync(deliveryId);

        return messages.Select(m => new MessageHistoryItemDto
        {
            SenderId = m.SenderId,
            Content = m.Content,
            SentAt = m.SentAt,
            IsRead = m.ReadAt != null
        }).ToList();
    }
}