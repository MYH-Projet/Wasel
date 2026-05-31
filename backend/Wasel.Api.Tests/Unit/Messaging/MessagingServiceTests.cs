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
using Xunit;

namespace Wasel.Api.Tests.Unit.Messaging;

public class MessagingServiceTests
{
    private readonly Mock<IMessageRepository> _messageRepositoryMock;
    private readonly Mock<IDeliveryRepository> _deliveryRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IDriverRepository> _driverRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly MessagingService _messagingService;

    public MessagingServiceTests()
    {
        _messageRepositoryMock = new Mock<IMessageRepository>();
        _deliveryRepositoryMock = new Mock<IDeliveryRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _driverRepositoryMock = new Mock<IDriverRepository>();
        _notificationServiceMock = new Mock<INotificationService>();

        _messagingService = new MessagingService(
            _messageRepositoryMock.Object,
            _deliveryRepositoryMock.Object,
            _userRepositoryMock.Object,
            _driverRepositoryMock.Object,
            _notificationServiceMock.Object
        );
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_WhenAdmin_ReturnsSuccessfully()
    {
        // Arrange
        var roles = new List<string> { KeycloakConstants.RoleAdmin };

        // Act
        await _messagingService.EnsureCanAccessDeliveryChatAsync(Guid.NewGuid(), "admin-keycloak-id", roles);

        // Assert
        // Should not throw any exception
        Assert.True(true);
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_WhenClientOwner_ReturnsSuccessfully()
    {
        // Arrange
        var keycloakId = "client-keycloak-id";
        var roles = new List<string> { "CLIENT" };
        var userId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var user = new User { Id = userId, KeycloakId = keycloakId };
        var delivery = new Delivery { Id = deliveryId, ClientId = userId };

        _userRepositoryMock.Setup(repo => repo.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _deliveryRepositoryMock.Setup(repo => repo.GetByIdAsync(deliveryId)).ReturnsAsync(delivery);

        // Act
        await _messagingService.EnsureCanAccessDeliveryChatAsync(deliveryId, keycloakId, roles);

        // Assert
        Assert.True(true); // Should not throw
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_WhenAssignedDriver_ReturnsSuccessfully()
    {
        // Arrange
        var keycloakId = "driver-keycloak-id";
        var roles = new List<string> { "DRIVER" };
        var userId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var user = new User { Id = userId, KeycloakId = keycloakId };
        var driver = new Driver { Id = driverId, UserId = userId };
        var delivery = new Delivery { Id = deliveryId, ClientId = Guid.NewGuid(), DriverId = driverId };

        _userRepositoryMock.Setup(repo => repo.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _deliveryRepositoryMock.Setup(repo => repo.GetByIdAsync(deliveryId)).ReturnsAsync(delivery);
        _driverRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(driver);

        // Act
        await _messagingService.EnsureCanAccessDeliveryChatAsync(deliveryId, keycloakId, roles);

        // Assert
        Assert.True(true); // Should not throw
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_WhenOtherUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var keycloakId = "other-keycloak-id";
        var roles = new List<string> { "CLIENT" };
        var userId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var user = new User { Id = userId, KeycloakId = keycloakId };
        var delivery = new Delivery { Id = deliveryId, ClientId = Guid.NewGuid(), DriverId = Guid.NewGuid() };

        _userRepositoryMock.Setup(repo => repo.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _deliveryRepositoryMock.Setup(repo => repo.GetByIdAsync(deliveryId)).ReturnsAsync(delivery);
        _driverRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync((Driver?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _messagingService.EnsureCanAccessDeliveryChatAsync(deliveryId, keycloakId, roles));
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_WhenDeliveryNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var keycloakId = "client-keycloak-id";
        var roles = new List<string> { "CLIENT" };
        var user = new User { Id = Guid.NewGuid(), KeycloakId = keycloakId };

        _userRepositoryMock.Setup(repo => repo.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _deliveryRepositoryMock.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Delivery?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _messagingService.EnsureCanAccessDeliveryChatAsync(Guid.NewGuid(), keycloakId, roles));
    }

    [Fact]
    public async Task EnsureCanAccessDeliveryChatAsync_DoesNotCompareDriverIdWithUserId()
    {
        // Arrange: Suppose delivery.DriverId happens to match user.Id (this is the specific bug we fixed)
        // If it was buggy, it would allow access. With the fix, it should check Driver.Id.
        var keycloakId = "sneaky-keycloak-id";
        var roles = new List<string> { "DRIVER" };
        var userId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        var user = new User { Id = userId, KeycloakId = keycloakId };
        // Delivery's DriverId = userId (which is WRONG in our domain but simulates the edge case)
        var delivery = new Delivery { Id = deliveryId, ClientId = Guid.NewGuid(), DriverId = userId };

        _userRepositoryMock.Setup(repo => repo.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _deliveryRepositoryMock.Setup(repo => repo.GetByIdAsync(deliveryId)).ReturnsAsync(delivery);
        
        // This user is NOT the driver of this delivery (their driver profile has a different DriverId)
        var driver = new Driver { Id = Guid.NewGuid(), UserId = userId };
        _driverRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(driver);

        // Act & Assert
        // Since driver.Id != delivery.DriverId (even though user.Id == delivery.DriverId), access must be denied.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _messagingService.EnsureCanAccessDeliveryChatAsync(deliveryId, keycloakId, roles));
    }
}
