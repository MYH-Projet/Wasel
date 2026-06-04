using Wasel.Api.Infrastructure.Firebase;
using Wasel.Api.Modules.Notifications.DTOs;
using Wasel.Api.Modules.Notifications.Entities;
using Wasel.Api.Modules.Notifications.Enums;
using Wasel.Api.Modules.Notifications.Repositories;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Shared.Exceptions;
using Wasel.Api.Shared.Security;

namespace Wasel.Api.Modules.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPushNotificationSender _pushSender;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        ICurrentUserService currentUserService,
        IPushNotificationSender pushSender,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
        _pushSender = pushSender;
        _logger = logger;
    }

    public async Task CreateAsync(Guid userId, NotificationType type, string title, string body)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            Status = NotificationStatus.UNREAD
        };

        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangesAsync();

        // Attempt FCM push — never block the caller
        try
        {
            await _pushSender.SendAsync(userId, title, body, type);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "FCM push failed for UserId={UserId}, Type={Type}. Notification persisted in DB.",
                userId, type);
        }
    }

    public async Task<NotificationsPageResponseDto> GetMyNotificationsAsync(int page, int pageSize)
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            throw new ApiException("Invalid token", 401);
        }

        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId)
            ?? throw new ApiException("User not found", 401);

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 50) pageSize = 50;

        var totalItems = await _notificationRepository.CountByUserIdAsync(user.Id);
        var unreadCount = await _notificationRepository.CountUnreadByUserIdAsync(user.Id);
        var notifications = await _notificationRepository.GetByUserIdPagedAsync(user.Id, page, pageSize);

        return new NotificationsPageResponseDto
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            UnreadCount = unreadCount,
            Items = notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Title = n.Title,
                Body = n.Body,
                Status = n.Status.ToString(),
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            })
        };
    }

    public async Task<NotificationResponseDto> MarkAsReadAsync(Guid notificationId)
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            throw new ApiException("Invalid token", 401);
        }

        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId)
            ?? throw new ApiException("User not found", 401);

        var notification = await _notificationRepository.GetByIdAsync(notificationId)
            ?? throw new ApiException("Notification not found", 404);

        if (notification.UserId != user.Id)
        {
            throw new ApiException("You do not own this notification", 403);
        }

        if (notification.Status != NotificationStatus.READ)
        {
            notification.Status = NotificationStatus.READ;
            notification.ReadAt = DateTime.UtcNow;
            await _notificationRepository.SaveChangesAsync();
        }

        return new NotificationResponseDto
        {
            Id = notification.Id,
            Type = notification.Type.ToString(),
            Title = notification.Title,
            Body = notification.Body,
            Status = notification.Status.ToString(),
            CreatedAt = notification.CreatedAt,
            ReadAt = notification.ReadAt
        };
    }

    public async Task<MarkAllReadResponseDto> MarkAllAsReadAsync()
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            throw new ApiException("Invalid token", 401);
        }

        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId)
            ?? throw new ApiException("User not found", 401);

        var updatedCount = await _notificationRepository.MarkAllReadAsync(user.Id);

        return new MarkAllReadResponseDto
        {
            UpdatedCount = updatedCount
        };
    }
}