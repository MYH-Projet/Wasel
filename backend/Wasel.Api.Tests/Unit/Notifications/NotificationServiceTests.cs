using Microsoft.Extensions.Logging;
using Moq;
using Wasel.Api.Infrastructure.Firebase;
using Wasel.Api.Modules.Notifications.Entities;
using Wasel.Api.Modules.Notifications.Enums;
using Wasel.Api.Modules.Notifications.Repositories;
using Wasel.Api.Modules.Notifications.Services;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Shared.Exceptions;
using Wasel.Api.Shared.Security;
using Xunit;

namespace Wasel.Api.Tests.Unit.Notifications;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IPushNotificationSender> _pushSenderMock;
    private readonly Mock<ILogger<NotificationService>> _loggerMock;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _pushSenderMock = new Mock<IPushNotificationSender>();
        _loggerMock = new Mock<ILogger<NotificationService>>();

        _sut = new NotificationService(
            _notificationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _pushSenderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_PersistsUnreadNotificationAndCallsPushSender()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var type = NotificationType.DELIVERY_ASSIGNED;
        var title = "Test Title";
        var body = "Test Body";

        _notificationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);
        _notificationRepositoryMock.Setup(r => r.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        _pushSenderMock.Setup(s => s.SendAsync(userId, title, body, type, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.CreateAsync(userId, type, title, body);

        // Assert
        _notificationRepositoryMock.Verify(r => r.AddAsync(It.Is<Notification>(n =>
            n.UserId == userId &&
            n.Type == type &&
            n.Title == title &&
            n.Body == body &&
            n.Status == NotificationStatus.UNREAD)), Times.Once);

        _notificationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        _pushSenderMock.Verify(s => s.SendAsync(userId, title, body, type, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenPushFails_DoesNotThrowAndKeepsNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var type = NotificationType.DRIVER_ARRIVING;
        var title = "Test Title";
        var body = "Test Body";

        _notificationRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Notification>())).Returns(Task.CompletedTask);
        _notificationRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        _pushSenderMock.Setup(s => s.SendAsync(userId, title, body, type, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("FCM Error"));

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _sut.CreateAsync(userId, type, title, body));

        Assert.Null(exception); // Should not throw

        _notificationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
        _notificationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetMyNotifications_ReturnsPagedNotificationsAndUnreadCount()
    {
        // Arrange
        var keycloakId = "user-kc-123";
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(c => c.KeycloakId).Returns(keycloakId);
        _userRepositoryMock.Setup(u => u.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(new User { Id = userId });

        _notificationRepositoryMock.Setup(r => r.CountByUserIdAsync(userId)).ReturnsAsync(5);
        _notificationRepositoryMock.Setup(r => r.CountUnreadByUserIdAsync(userId)).ReturnsAsync(2);
        
        var list = new List<Notification>
        {
            new Notification { Id = Guid.NewGuid(), Title = "A", Status = NotificationStatus.UNREAD },
            new Notification { Id = Guid.NewGuid(), Title = "B", Status = NotificationStatus.READ }
        };
        _notificationRepositoryMock.Setup(r => r.GetByUserIdPagedAsync(userId, 1, 20)).ReturnsAsync(list);

        // Act
        var result = await _sut.GetMyNotificationsAsync(1, 20);

        // Assert
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(5, result.TotalItems);
        Assert.Equal(2, result.UnreadCount);
        Assert.Equal(2, result.Items.Count());
    }

    [Fact]
    public async Task MarkAsRead_WhenOwner_MarksRead()
    {
        // Arrange
        var keycloakId = "user-kc-123";
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(c => c.KeycloakId).Returns(keycloakId);
        _userRepositoryMock.Setup(u => u.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(new User { Id = userId });

        var notification = new Notification { Id = notificationId, UserId = userId, Status = NotificationStatus.UNREAD };
        _notificationRepositoryMock.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);
        _notificationRepositoryMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.MarkAsReadAsync(notificationId);

        // Assert
        Assert.Equal(NotificationStatus.READ.ToString(), result.Status);
        Assert.NotNull(result.ReadAt);
        _notificationRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotOwner_ThrowsForbidden()
    {
        // Arrange
        var keycloakId = "user-kc-123";
        var userId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(c => c.KeycloakId).Returns(keycloakId);
        _userRepositoryMock.Setup(u => u.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(new User { Id = userId });

        var notification = new Notification { Id = notificationId, UserId = anotherUserId, Status = NotificationStatus.UNREAD };
        _notificationRepositoryMock.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => _sut.MarkAsReadAsync(notificationId));
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotFound_ThrowsNotFound()
    {
        // Arrange
        var keycloakId = "user-kc-123";
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(c => c.KeycloakId).Returns(keycloakId);
        _userRepositoryMock.Setup(u => u.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(new User { Id = userId });

        _notificationRepositoryMock.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync((Notification?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => _sut.MarkAsReadAsync(notificationId));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task MarkAllAsRead_OnlyCurrentUserNotifications()
    {
        // Arrange
        var keycloakId = "user-kc-123";
        var userId = Guid.NewGuid();
        
        _currentUserServiceMock.Setup(c => c.KeycloakId).Returns(keycloakId);
        _userRepositoryMock.Setup(u => u.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(new User { Id = userId });

        _notificationRepositoryMock.Setup(r => r.MarkAllReadAsync(userId)).ReturnsAsync(3);

        // Act
        var result = await _sut.MarkAllAsReadAsync();

        // Assert
        Assert.Equal(3, result.UpdatedCount);
        _notificationRepositoryMock.Verify(r => r.MarkAllReadAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Pagination_LimitsPageSizeTo50()
    {
        // Arrange
        var keycloakId = "user-kc-123";
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(c => c.KeycloakId).Returns(keycloakId);
        _userRepositoryMock.Setup(u => u.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(new User { Id = userId });

        _notificationRepositoryMock.Setup(r => r.CountByUserIdAsync(userId)).ReturnsAsync(0);
        _notificationRepositoryMock.Setup(r => r.CountUnreadByUserIdAsync(userId)).ReturnsAsync(0);
        _notificationRepositoryMock.Setup(r => r.GetByUserIdPagedAsync(userId, 1, 50)).ReturnsAsync(new List<Notification>());

        // Act
        var result = await _sut.GetMyNotificationsAsync(1, 100); // Requesting 100

        // Assert
        Assert.Equal(50, result.PageSize); // Should be limited to 50
        _notificationRepositoryMock.Verify(r => r.GetByUserIdPagedAsync(userId, 1, 50), Times.Once);
    }
}
