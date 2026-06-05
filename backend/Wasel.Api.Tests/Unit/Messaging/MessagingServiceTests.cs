using Microsoft.Extensions.Options;
using Moq;
using Wasel.Api.Infrastructure.Keycloak;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Deliveries.Repositories;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Messaging.Repositories;
using Wasel.Api.Modules.Messaging.Services;
using Wasel.Api.Modules.Notifications.Services;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Shared.EventBus;
using Xunit;

namespace Wasel.Api.Tests.Unit.Messaging;

public class MessagingServiceTests
{
    private readonly Mock<IMessageRepository> _messageRepositoryMock;
    private readonly Mock<IDeliveryRepository> _deliveryRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IEventBus> _eventBusMock;
    private readonly IOptions<RabbitMqOptions> _rabbitMqOptions;
    private readonly MessagingService _messagingService;

    public MessagingServiceTests()
    {
        _messageRepositoryMock = new Mock<IMessageRepository>();
        _deliveryRepositoryMock = new Mock<IDeliveryRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _eventBusMock = new Mock<IEventBus>();

        _rabbitMqOptions = Options.Create(new RabbitMqOptions
        {
            Host = "localhost",
            Port = 5672,
            Username = "guest",
            Password = "guest",
            ExchangeName = "wasel.events",
            NotificationRoutingKey = "notification.requested"
        });

        _messagingService = new MessagingService(
            _messageRepositoryMock.Object,
            _deliveryRepositoryMock.Object,
            _userRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _notificationServiceMock.Object,
            _eventBusMock.Object,
            _rabbitMqOptions
        );
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_WhenAdmin_ReturnsSuccessfully()
    {
        var roles = new List<string> { KeycloakConstants.RoleAdmin };

        await _messagingService.EnsureCanAccessDeliveryChatAsync(
            Guid.NewGuid(),
            "admin-keycloak-id",
            roles);

        Assert.True(true);
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_WhenClientOwner_ReturnsSuccessfully()
    {
        var keycloakId = "client-keycloak-id";
        var roles = new List<string> { "CLIENT" };
        var userId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var user = new User { Id = userId, KeycloakId = keycloakId };
        var delivery = new Delivery { Id = deliveryId, ClientId = userId };

        _userRepositoryMock
            .Setup(repo => repo.GetByKeycloakIdAsync(keycloakId))
            .ReturnsAsync(user);

        _deliveryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(deliveryId))
            .ReturnsAsync(delivery);

        await _messagingService.EnsureCanAccessDeliveryChatAsync(
            deliveryId,
            keycloakId,
            roles);

        Assert.True(true);
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_WhenAssignedDriver_ReturnsSuccessfully()
    {
        var keycloakId = "driver-keycloak-id";
        var roles = new List<string> { "DRIVER" };
        var userId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var user = new User { Id = userId, KeycloakId = keycloakId };
        var driver = new Driver { Id = driverId, UserId = userId };
        var delivery = new Delivery
        {
            Id = deliveryId,
            ClientId = Guid.NewGuid(),
            DriverId = driverId
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByKeycloakIdAsync(keycloakId))
            .ReturnsAsync(user);

        _deliveryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(deliveryId))
            .ReturnsAsync(delivery);

        _driverRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(driver);

        await _messagingService.EnsureCanAccessDeliveryChatAsync(
            deliveryId,
            keycloakId,
            roles);

        Assert.True(true);
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_WhenOtherUser_ThrowsUnauthorizedAccessException()
    {
        var keycloakId = "other-keycloak-id";
        var roles = new List<string> { "CLIENT" };
        var userId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var user = new User { Id = userId, KeycloakId = keycloakId };
        var delivery = new Delivery
        {
            Id = deliveryId,
            ClientId = Guid.NewGuid(),
            DriverId = Guid.NewGuid()
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByKeycloakIdAsync(keycloakId))
            .ReturnsAsync(user);

        _deliveryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(deliveryId))
            .ReturnsAsync(delivery);

        _driverRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync((Driver?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _messagingService.EnsureCanAccessDeliveryChatAsync(
                deliveryId,
                keycloakId,
                roles));
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_WhenDeliveryNotFound_ThrowsInvalidOperationException()
    {
        var keycloakId = "client-keycloak-id";
        var roles = new List<string> { "CLIENT" };
        var user = new User
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByKeycloakIdAsync(keycloakId))
            .ReturnsAsync(user);

        _deliveryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Delivery?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _messagingService.EnsureCanAccessDeliveryChatAsync(
                Guid.NewGuid(),
                keycloakId,
                roles));
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_DoesNotCompareDriverIdWithUserId()
    {
        var keycloakId = "sneaky-keycloak-id";
        var roles = new List<string> { "DRIVER" };
        var userId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var user = new User
        {
            Id = userId,
            KeycloakId = keycloakId
        };

        var delivery = new Delivery
        {
            Id = deliveryId,
            ClientId = Guid.NewGuid(),
            DriverId = userId
        };

        var driver = new Driver
        {
            Id = Guid.NewGuid(),
            UserId = userId
        };

        _userRepositoryMock
            .Setup(repo => repo.GetByKeycloakIdAsync(keycloakId))
            .ReturnsAsync(user);

        _deliveryRepositoryMock
            .Setup(repo => repo.GetByIdAsync(deliveryId))
            .ReturnsAsync(delivery);

        _driverRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId))
            .ReturnsAsync(driver);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _messagingService.EnsureCanAccessDeliveryChatAsync(
                deliveryId,
                keycloakId,
                roles));
    }
}