using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Wasel.Api.Modules.Auth.DTOs;
using Wasel.Api.Modules.Auth.Services;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Modules.Tracking.DTOs;
using Wasel.Api.Modules.Tracking.Entities;
using Wasel.Api.Modules.Tracking.Repositories;
using Wasel.Api.Modules.Tracking.Services;
using Wasel.Api.Shared.Exceptions;
using Xunit;

namespace Wasel.Api.Tests.Unit.Tracking;

public class TrackingServiceTests
{
    private readonly Mock<ITrackingRepository> _trackingRepositoryMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly IMemoryCache _memoryCache;
    private readonly TrackingService _trackingService;

    public TrackingServiceTests()
    {
        _trackingRepositoryMock = new Mock<ITrackingRepository>();
        _authServiceMock = new Mock<IAuthService>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _trackingService = new TrackingService(_trackingRepositoryMock.Object, _authServiceMock.Object, _memoryCache);
    }

    [Fact]
    public async Task UpdateCurrentDriverPositionAsync_UserNotFound_ThrowsUnauthorized()
    {
        _authServiceMock.Setup(x => x.EnsureCurrentUserExistsAsync())
            .ReturnsAsync(new CurrentUserResponseDto { LocalUserId = null });

        var dto = new TrackingPointUpdateDto { Latitude = 10, Longitude = 10 };

        var act = async () => await _trackingService.UpdateCurrentDriverPositionAsync(dto);
        
        await act.Should().ThrowAsync<ApiException>().WithMessage("*introuvable*");
    }

    [Fact]
    public async Task UpdateCurrentDriverPositionAsync_DriverNotFound_ThrowsForbidden()
    {
        var userId = Guid.NewGuid();
        _authServiceMock.Setup(x => x.EnsureCurrentUserExistsAsync())
            .ReturnsAsync(new CurrentUserResponseDto { LocalUserId = userId });

        _trackingRepositoryMock.Setup(x => x.GetDriverByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Driver)null);

        var dto = new TrackingPointUpdateDto { Latitude = 10, Longitude = 10 };

        var act = async () => await _trackingService.UpdateCurrentDriverPositionAsync(dto);
        
        await act.Should().ThrowAsync<ApiException>().WithMessage("*pas de profil*");
    }

    [Fact]
    public async Task UpdateCurrentDriverPositionAsync_ValidDriver_FirstTime_Persists()
    {
        var userId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        _authServiceMock.Setup(x => x.EnsureCurrentUserExistsAsync())
            .ReturnsAsync(new CurrentUserResponseDto { LocalUserId = userId });

        var driver = new Driver { Id = driverId, UserId = userId, Status = DriverStatus.Approved };
        _trackingRepositoryMock.Setup(x => x.GetDriverByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        var dto = new TrackingPointUpdateDto 
        { 
            Latitude = 33.5, 
            Longitude = -7.5, 
            DeliveryId = deliveryId 
        };

        _trackingRepositoryMock.Setup(x => x.AddTrackingPointAsync(It.IsAny<TrackingPoint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrackingPoint p, CancellationToken c) => { p.Id = Guid.NewGuid(); return p; });

        var result = await _trackingService.UpdateCurrentDriverPositionAsync(dto);

        result.Should().NotBeNull();
        result.Persisted.Should().BeTrue();
        result.Id.Should().NotBeNull();
        result.DriverId.Should().Be(driverId);
        
        // Ensure repository was called
        _trackingRepositoryMock.Verify(x => x.AddTrackingPointAsync(It.IsAny<TrackingPoint>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task UpdateCurrentDriverPositionAsync_ValidDriver_Under5Seconds_DoesNotPersist()
    {
        var userId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();

        _authServiceMock.Setup(x => x.EnsureCurrentUserExistsAsync())
            .ReturnsAsync(new CurrentUserResponseDto { LocalUserId = userId });

        var driver = new Driver { Id = driverId, UserId = userId, Status = DriverStatus.Approved };
        _trackingRepositoryMock.Setup(x => x.GetDriverByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(driver);

        var dto = new TrackingPointUpdateDto { Latitude = 33.5, Longitude = -7.5, DeliveryId = deliveryId };

        _trackingRepositoryMock.Setup(x => x.AddTrackingPointAsync(It.IsAny<TrackingPoint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TrackingPoint p, CancellationToken c) => { p.Id = Guid.NewGuid(); return p; });

        // First call
        await _trackingService.UpdateCurrentDriverPositionAsync(dto);
        
        // Second call immediately after
        var result2 = await _trackingService.UpdateCurrentDriverPositionAsync(dto);

        result2.Should().NotBeNull();
        result2.Persisted.Should().BeFalse();
        result2.Id.Should().BeNull(); // Not persisted so no DB ID
        
        // Ensure repository was only called ONCE despite two service calls
        _trackingRepositoryMock.Verify(x => x.AddTrackingPointAsync(It.IsAny<TrackingPoint>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
