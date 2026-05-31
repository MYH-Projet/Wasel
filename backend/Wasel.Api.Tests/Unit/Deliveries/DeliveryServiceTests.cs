using Moq;
using Wasel.Api.Modules.Deliveries.DTOs;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Deliveries.Repositories;
using Wasel.Api.Modules.Deliveries.Services;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Notifications.Services;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Shared.Database;
using Xunit;

namespace Wasel.Api.Tests.Unit.Deliveries;

public class DeliveryServiceTests
{
    private readonly Mock<IDeliveryRepository> _mockDeliveryRepository;
    private readonly Mock<IDriverRepository> _mockDriverRepository;
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly DeliveryService _deliveryService;

    public DeliveryServiceTests()
    {
        _mockDeliveryRepository = new Mock<IDeliveryRepository>();
        _mockDriverRepository = new Mock<IDriverRepository>();
        _mockNotificationService = new Mock<INotificationService>();
        // Note: WaselDbContext is not fully mocked here because we only test methods that do not use it directly.
        _deliveryService = new DeliveryService(
            _mockDeliveryRepository.Object,
            _mockDriverRepository.Object,
            null!, // _context
            _mockNotificationService.Object);
    }

    [Fact]
    public void EstimateDelivery_WhenValidInput_ReturnsPriceAndBreakdown()
    {
        // Arrange
        var request = new DeliveryEstimateRequestDto
        {
            PickupLat = 33.5731, PickupLng = -7.5898,
            DropoffLat = 34.0209, DropoffLng = -6.8416,
            Weight = 2.5, IsFragile = false
        };

        // Act
        var result = _deliveryService.EstimateDelivery(request);

        // Assert
        Assert.Equal("MAD", result.Currency);
        Assert.Equal(10m, result.Breakdown.BaseFee);
        Assert.Equal(5m, result.Breakdown.WeightFee);
        Assert.Equal(0m, result.Breakdown.FragileFee);
        Assert.True(result.EstimatedPrice > 0);
        Assert.True(result.DistanceKm > 0);
    }

    [Fact]
    public void EstimateDelivery_WhenWeightInvalid_ThrowsBadRequest()
    {
        var request = new DeliveryEstimateRequestDto { Weight = 0 };
        Assert.Throws<ArgumentException>(() => _deliveryService.EstimateDelivery(request));
    }

    [Fact]
    public void EstimateDelivery_WhenCoordinatesInvalid_ThrowsBadRequest()
    {
        var request = new DeliveryEstimateRequestDto { PickupLat = 95, Weight = 1 };
        Assert.Throws<ArgumentException>(() => _deliveryService.EstimateDelivery(request));
    }

    [Fact]
    public void EstimateDelivery_WhenFragile_AddsFragileFee()
    {
        var request = new DeliveryEstimateRequestDto { PickupLat = 33, PickupLng = -7, DropoffLat = 34, DropoffLng = -6, Weight = 2, IsFragile = true };
        var result = _deliveryService.EstimateDelivery(request);
        Assert.Equal(5m, result.Breakdown.FragileFee);
    }

    [Fact]
    public void EstimateDelivery_CalculatesDistanceWithHaversine()
    {
        var request = new DeliveryEstimateRequestDto { PickupLat = 0, PickupLng = 0, DropoffLat = 0, DropoffLng = 1, Weight = 1 }; // ~111 km
        var result = _deliveryService.EstimateDelivery(request);
        Assert.True(result.DistanceKm > 110 && result.DistanceKm < 112);
    }

    [Fact]
    public async Task GetMyActiveDeliveries_WhenClient_ReturnsOnlyClientActiveDeliveries()
    {
        var userId = Guid.NewGuid();
        _mockDeliveryRepository.Setup(r => r.GetUserByKeycloakIdAsync("test")).ReturnsAsync(new User { Id = userId });
        _mockDriverRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Driver?)null);
        
        var activeDeliveries = new List<Delivery>
        {
            new Delivery { Id = Guid.NewGuid(), ClientId = userId, Status = DeliveryStatus.CREATED, PickupAddress = new Address(), DropoffAddress = new Address() }
        };
        _mockDeliveryRepository.Setup(r => r.GetActiveDeliveriesForUserAsync(userId, null, It.IsAny<IEnumerable<DeliveryStatus>>()))
            .ReturnsAsync(activeDeliveries);

        var result = await _deliveryService.GetMyActiveDeliveriesAsync("test");
        Assert.Single(result);
        Assert.Equal(activeDeliveries[0].Id, result[0].Id);
    }

    [Fact]
    public async Task GetMyActiveDeliveries_WhenDriver_ReturnsOnlyAssignedDriverActiveDeliveries()
    {
        var userId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        _mockDeliveryRepository.Setup(r => r.GetUserByKeycloakIdAsync("test")).ReturnsAsync(new User { Id = userId });
        _mockDriverRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(new Driver { Id = driverId });
        
        var activeDeliveries = new List<Delivery>
        {
            new Delivery { Id = Guid.NewGuid(), DriverId = driverId, Status = DeliveryStatus.ACCEPTED, PickupAddress = new Address(), DropoffAddress = new Address() }
        };
        _mockDeliveryRepository.Setup(r => r.GetActiveDeliveriesForUserAsync(userId, driverId, It.IsAny<IEnumerable<DeliveryStatus>>()))
            .ReturnsAsync(activeDeliveries);

        var result = await _deliveryService.GetMyActiveDeliveriesAsync("test");
        Assert.Single(result);
        Assert.Equal(activeDeliveries[0].Id, result[0].Id);
    }

    [Fact]
    public async Task GetDeliveryDetailAsync_WhenAssignedDriver_ReturnsDetail()
    {
        var deliveryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        
        var delivery = new Delivery { Id = deliveryId, DriverId = driverId, ClientId = Guid.NewGuid(), PickupAddress = new Address(), DropoffAddress = new Address() };
        
        _mockDeliveryRepository.Setup(r => r.GetDeliveryWithDetailsAsync(deliveryId)).ReturnsAsync(delivery);
        _mockDeliveryRepository.Setup(r => r.GetUserByKeycloakIdAsync("test")).ReturnsAsync(new User { Id = userId });
        _mockDriverRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(new Driver { Id = driverId });

        var result = await _deliveryService.GetDeliveryDetailAsync(deliveryId, "test", Enumerable.Empty<string>());
        
        Assert.NotNull(result);
        Assert.Equal(deliveryId, result.Id);
    }

    [Fact]
    public async Task GetDeliveryDetailAsync_WhenOtherDriver_ThrowsUnauthorized()
    {
        var deliveryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherDriverId = Guid.NewGuid();
        var assignedDriverId = Guid.NewGuid();
        
        var delivery = new Delivery { Id = deliveryId, DriverId = assignedDriverId, ClientId = Guid.NewGuid() };
        
        _mockDeliveryRepository.Setup(r => r.GetDeliveryWithDetailsAsync(deliveryId)).ReturnsAsync(delivery);
        _mockDeliveryRepository.Setup(r => r.GetUserByKeycloakIdAsync("test")).ReturnsAsync(new User { Id = userId });
        _mockDriverRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(new Driver { Id = otherDriverId });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _deliveryService.GetDeliveryDetailAsync(deliveryId, "test", Enumerable.Empty<string>()));
    }

    [Fact]
    public async Task GetDeliveryDetailAsync_WhenDriverIdEqualsUserId_ThrowsUnauthorized()
    {
        var deliveryId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        // This is to simulate the previous bug where we compared DriverId with UserId
        // Here, the delivery.DriverId is exactly the User.Id, but we shouldn't authorize unless it matches Driver.Id
        var delivery = new Delivery { Id = deliveryId, DriverId = userId, ClientId = Guid.NewGuid() };
        
        _mockDeliveryRepository.Setup(r => r.GetDeliveryWithDetailsAsync(deliveryId)).ReturnsAsync(delivery);
        _mockDeliveryRepository.Setup(r => r.GetUserByKeycloakIdAsync("test")).ReturnsAsync(new User { Id = userId });
        // Driver is null (or has a different ID)
        _mockDriverRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Driver?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _deliveryService.GetDeliveryDetailAsync(deliveryId, "test", Enumerable.Empty<string>()));
    }
}
