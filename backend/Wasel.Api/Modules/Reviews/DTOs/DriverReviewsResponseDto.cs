namespace Wasel.Api.Modules.Reviews.DTOs;

public class DriverReviewsResponseDto
{
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<ReviewListItemDto> Items { get; set; } = new List<ReviewListItemDto>();
}
