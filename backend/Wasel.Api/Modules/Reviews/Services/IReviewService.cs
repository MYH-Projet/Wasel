using Wasel.Api.Modules.Reviews.DTOs;

namespace Wasel.Api.Modules.Reviews.Services;

public interface IReviewService
{
    Task<ReviewResponseDto> CreateReviewAsync(CreateReviewRequestDto request);
    Task<DriverReviewsResponseDto> GetDriverReviewsAsync(Guid driverId, int page, int pageSize);
}
