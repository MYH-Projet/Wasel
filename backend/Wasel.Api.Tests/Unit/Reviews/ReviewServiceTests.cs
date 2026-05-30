using Moq;
using Wasel.Api.Modules.Deliveries.Entities;
using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Deliveries.Repositories;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Reviews.DTOs;
using Wasel.Api.Modules.Reviews.Entities;
using Wasel.Api.Modules.Reviews.Repositories;
using Wasel.Api.Modules.Reviews.Services;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Shared.Exceptions;
using Wasel.Api.Shared.Security;
using Xunit;

namespace Wasel.Api.Tests.Unit.Reviews;

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _mockReviewRepository;
    private readonly Mock<IDeliveryRepository> _mockDeliveryRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IDriverRepository> _mockDriverRepository;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly ReviewService _reviewService;

    public ReviewServiceTests()
    {
        _mockReviewRepository = new Mock<IReviewRepository>();
        _mockDeliveryRepository = new Mock<IDeliveryRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockDriverRepository = new Mock<IDriverRepository>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();

        _reviewService = new ReviewService(
            _mockReviewRepository.Object,
            _mockDeliveryRepository.Object,
            _mockUserRepository.Object,
            _mockDriverRepository.Object,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task CreateReview_WhenDeliveryDeliveredAndOwner_CreatesReview()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var driverId = Guid.NewGuid();
        var deliveryId = Guid.NewGuid();
        var keycloakId = "test-kc-id";

        _mockCurrentUserService.Setup(s => s.KeycloakId).Returns(keycloakId);
        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(new User { Id = userId });
        
        var delivery = new Delivery
        {
            Id = deliveryId,
            ClientId = userId,
            DriverId = driverId,
            Status = DeliveryStatus.DELIVERED
        };
        _mockDeliveryRepository.Setup(r => r.GetByIdAsync(deliveryId)).ReturnsAsync(delivery);
        _mockReviewRepository.Setup(r => r.ExistsByDeliveryIdAsync(deliveryId)).ReturnsAsync(false);

        var request = new CreateReviewRequestDto { DeliveryId = deliveryId, Rating = 5, Comment = "Great" };

        // Act
        var result = await _reviewService.CreateReviewAsync(request);

        // Assert
        Assert.Equal(5, result.Rating);
        Assert.Equal("Great", result.Comment);
        _mockReviewRepository.Verify(r => r.AddAsync(It.IsAny<Review>()), Times.Once);
        _mockReviewRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateReview_WhenDeliveryNotFound_ThrowsNotFound()
    {
        // Arrange
        var keycloakId = "test-kc-id";
        _mockCurrentUserService.Setup(s => s.KeycloakId).Returns(keycloakId);
        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(new User { Id = Guid.NewGuid() });
        
        _mockDeliveryRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Delivery?)null);

        var request = new CreateReviewRequestDto { DeliveryId = Guid.NewGuid(), Rating = 5 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => _reviewService.CreateReviewAsync(request));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task CreateReview_WhenUserNotOwner_ThrowsForbidden()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var keycloakId = "test-kc-id";
        _mockCurrentUserService.Setup(s => s.KeycloakId).Returns(keycloakId);
        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(new User { Id = userId });
        
        var delivery = new Delivery { ClientId = Guid.NewGuid() }; // Different client
        _mockDeliveryRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(delivery);

        var request = new CreateReviewRequestDto { DeliveryId = Guid.NewGuid(), Rating = 5 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => _reviewService.CreateReviewAsync(request));
        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task CreateReview_WhenDeliveryNotDelivered_ThrowsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var keycloakId = "test-kc-id";
        _mockCurrentUserService.Setup(s => s.KeycloakId).Returns(keycloakId);
        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(new User { Id = userId });
        
        var delivery = new Delivery { ClientId = userId, Status = DeliveryStatus.IN_TRANSIT }; 
        _mockDeliveryRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(delivery);

        var request = new CreateReviewRequestDto { DeliveryId = Guid.NewGuid(), Rating = 5 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => _reviewService.CreateReviewAsync(request));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task CreateReview_WhenReviewAlreadyExists_ThrowsConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var keycloakId = "test-kc-id";
        var deliveryId = Guid.NewGuid();

        _mockCurrentUserService.Setup(s => s.KeycloakId).Returns(keycloakId);
        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(new User { Id = userId });
        
        var delivery = new Delivery { Id = deliveryId, ClientId = userId, DriverId = Guid.NewGuid(), Status = DeliveryStatus.DELIVERED }; 
        _mockDeliveryRepository.Setup(r => r.GetByIdAsync(deliveryId)).ReturnsAsync(delivery);
        
        _mockReviewRepository.Setup(r => r.ExistsByDeliveryIdAsync(deliveryId)).ReturnsAsync(true);

        var request = new CreateReviewRequestDto { DeliveryId = deliveryId, Rating = 5 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => _reviewService.CreateReviewAsync(request));
        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task GetDriverReviews_WhenDriverExists_ReturnsAverageTotalAndPagedItems()
    {
        // Arrange
        var driverId = Guid.NewGuid();
        _mockDriverRepository.Setup(r => r.ExistsByIdAsync(driverId)).ReturnsAsync(true);
        
        _mockReviewRepository.Setup(r => r.CountByDriverIdAsync(driverId)).ReturnsAsync(2);
        _mockReviewRepository.Setup(r => r.AverageRatingByDriverIdAsync(driverId)).ReturnsAsync(4.5);
        
        var reviews = new List<Review>
        {
            new Review { Rating = 5, Comment = "Good", CreatedAt = DateTime.UtcNow, ReviewerUser = new User { FirstName = "John" } },
            new Review { Rating = 4, Comment = "Ok", CreatedAt = DateTime.UtcNow, ReviewerUser = new User { FirstName = "Jane" } }
        };
        
        _mockReviewRepository.Setup(r => r.GetByDriverIdPagedAsync(driverId, 1, 10)).ReturnsAsync(reviews);

        // Act
        var result = await _reviewService.GetDriverReviewsAsync(driverId, 1, 10);

        // Assert
        Assert.Equal(4.5, result.AverageRating);
        Assert.Equal(2, result.TotalReviews);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal("John", result.Items.First().ReviewerFirstName);
    }

    [Fact]
    public async Task GetDriverReviews_WhenDriverNotFound_ThrowsNotFound()
    {
        // Arrange
        _mockDriverRepository.Setup(r => r.ExistsByIdAsync(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() => _reviewService.GetDriverReviewsAsync(Guid.NewGuid(), 1, 10));
        Assert.Equal(404, ex.StatusCode);
    }
}
