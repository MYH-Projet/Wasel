using Wasel.Api.Modules.Deliveries.Enums;
using Wasel.Api.Modules.Deliveries.Repositories;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Reviews.DTOs;
using Wasel.Api.Modules.Reviews.Entities;
using Wasel.Api.Modules.Reviews.Repositories;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Shared.Exceptions;
using Wasel.Api.Shared.Security;

namespace Wasel.Api.Modules.Reviews.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IDeliveryRepository _deliveryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IDriverRepository _driverRepository;
    private readonly ICurrentUserService _currentUserService;

    public ReviewService(
        IReviewRepository reviewRepository,
        IDeliveryRepository deliveryRepository,
        IUserRepository userRepository,
        IDriverRepository driverRepository,
        ICurrentUserService currentUserService)
    {
        _reviewRepository = reviewRepository;
        _deliveryRepository = deliveryRepository;
        _userRepository = userRepository;
        _driverRepository = driverRepository;
        _currentUserService = currentUserService;
    }

    public async Task<ReviewResponseDto> CreateReviewAsync(CreateReviewRequestDto request)
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (string.IsNullOrEmpty(keycloakId))
        {
            throw new ApiException("Invalid token", 401);
        }

        var user = await _userRepository.GetByKeycloakIdAsync(keycloakId)
            ?? throw new ApiException("User not found", 401);

        var delivery = await _deliveryRepository.GetByIdAsync(request.DeliveryId)
            ?? throw new ApiException("Delivery not found", 404);

        if (delivery.ClientId != user.Id)
        {
            throw new ApiException("You can only review your own deliveries", 403);
        }

        if (delivery.Status != DeliveryStatus.DELIVERED)
        {
            throw new ApiException("Delivery must be DELIVERED to be reviewed", 400);
        }

        if (!delivery.DriverId.HasValue)
        {
            throw new ApiException("No driver assigned to this delivery", 400);
        }

        var reviewExists = await _reviewRepository.ExistsByDeliveryIdAsync(request.DeliveryId);
        if (reviewExists)
        {
            throw new ApiException("A review already exists for this delivery", 409);
        }

        var review = new Review
        {
            DeliveryId = request.DeliveryId,
            ReviewerUserId = user.Id,
            ReviewedDriverId = delivery.DriverId.Value,
            Rating = request.Rating,
            Comment = request.Comment
        };

        await _reviewRepository.AddAsync(review);
        await _reviewRepository.SaveChangesAsync();

        return new ReviewResponseDto
        {
            Id = review.Id,
            DeliveryId = review.DeliveryId,
            ReviewerUserId = review.ReviewerUserId,
            ReviewedDriverId = review.ReviewedDriverId,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt
        };
    }

    public async Task<DriverReviewsResponseDto> GetDriverReviewsAsync(Guid driverId, int page, int pageSize)
    {
        var driverExists = await _driverRepository.ExistsByIdAsync(driverId);
        if (!driverExists)
        {
            throw new ApiException("Driver not found", 404);
        }

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        var totalReviews = await _reviewRepository.CountByDriverIdAsync(driverId);
        var averageRating = await _reviewRepository.AverageRatingByDriverIdAsync(driverId);
        var reviews = await _reviewRepository.GetByDriverIdPagedAsync(driverId, page, pageSize);

        var items = reviews.Select(r => new ReviewListItemDto
        {
            Rating = r.Rating,
            Comment = r.Comment,
            Date = r.CreatedAt,
            ReviewerFirstName = r.ReviewerUser.FirstName
        });

        return new DriverReviewsResponseDto
        {
            AverageRating = Math.Round(averageRating, 1),
            TotalReviews = totalReviews,
            Page = page,
            PageSize = pageSize,
            Items = items
        };
    }
}