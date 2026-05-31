using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Deliveries.Repositories;
using Wasel.Api.Modules.Messaging.DTOs;
using Wasel.Api.Modules.Messaging.Entities;
using Wasel.Api.Modules.Messaging.Repositories;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Notifications.Enums;
using Wasel.Api.Modules.Notifications.Services;

namespace Wasel.Api.Modules.Messaging.Services;

public class MessagingService : IMessagingService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly INotificationService _notificationService;

    public MessagingService(
        IMessageRepository messageRepository,
        IDeliveryRepository deliveryRepository,
        IUserRepository userRepository,
        IDriverRepository driverRepository,
        INotificationService notificationService)
    {
        _messageRepository = messageRepository;
        _deliveryRepository = deliveryRepository;
        _userRepository = userRepository;
        _driverRepository = driverRepository;
        _notificationService = notificationService;
    }

    public async Task<object> SendMessageAsync(SendMessageRequestDto request, string keycloakId)
    {
        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);

        if (user is null)
            throw new UnauthorizedAccessException("Utilisateur introuvable.");

        var delivery = await _deliveryRepository.GetByIdAsync(request.DeliveryId);

        if (delivery is null)
            throw new InvalidOperationException("Livraison introuvable.");

        var driver = await _driverRepository.GetByUserIdAsync(user.Id);
        bool isDriver = driver != null && delivery.DriverId == driver.Id;

        if (delivery.ClientId != user.Id && !isDriver)
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

        // Notify recipient non-blocking
        try
        {
            Guid? recipientUserId = null;

            if (user.Id == delivery.ClientId)
            {
                // Sender is Client, notify Driver
                if (delivery.DriverId.HasValue)
                {
                    var recipientDriver = await _driverRepository.GetByIdAsync(delivery.DriverId.Value);
                    if (recipientDriver != null)
                    {
                        recipientUserId = recipientDriver.UserId;
                    }
                }
            }
            else
            {
                // Sender is Driver, notify Client
                recipientUserId = delivery.ClientId;
            }

            if (recipientUserId.HasValue)
            {
                await _notificationService.CreateAsync(
                    recipientUserId.Value,
                    NotificationType.NEW_MESSAGE,
                    "Nouveau message",
                    "Vous avez reçu un nouveau message.");
            }
        }
        catch { /* notification failure must not affect message creation */ }

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

        var driver = await _driverRepository.GetByUserIdAsync(user.Id);
        bool isDriver = driver != null && delivery.DriverId == driver.Id;

        if (delivery.ClientId != user.Id && !isDriver)
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

    public async Task EnsureCanAccessDeliveryChatAsync(Guid deliveryId, string keycloakId, IEnumerable<string> roles)
    {
        if (roles.Contains(Wasel.Api.Infrastructure.Keycloak.KeycloakConstants.RoleAdmin))
            return;

        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId);
        if (user == null)
            throw new UnauthorizedAccessException("Utilisateur introuvable.");

        var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
        if (delivery == null)
            throw new InvalidOperationException("Livraison introuvable.");

        if (delivery.ClientId == user.Id)
            return;

        var driver = await _driverRepository.GetByUserIdAsync(user.Id);
        if (driver != null && delivery.DriverId == driver.Id)
            return;

        throw new UnauthorizedAccessException("Vous n'êtes pas autorisé à rejoindre ce chat.");
    }
}