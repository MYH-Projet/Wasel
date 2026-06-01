using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Reviews.DTOs;
using Wasel.Api.Modules.Reviews.Services;
using Wasel.Api.Shared.Exceptions;

namespace Wasel.Api.Modules.Reviews.Controllers;

[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost("api/reviews")]
    [Authorize(Policy = "ActiveUserOnly")]
    public async Task<ActionResult<ReviewResponseDto>> CreateReview([FromBody] CreateReviewRequestDto request)
    {
        try
        {
            var result = await _reviewService.CreateReviewAsync(request);
            return Ok(result);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpGet("api/drivers/{driverId:guid}/reviews")]
    public async Task<ActionResult<DriverReviewsResponseDto>> GetDriverReviews(
        Guid driverId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _reviewService.GetDriverReviewsAsync(driverId, page, pageSize);
            return Ok(result);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }
}
