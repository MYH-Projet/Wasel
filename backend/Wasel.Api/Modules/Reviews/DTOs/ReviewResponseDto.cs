namespace Wasel.Api.Modules.Reviews.DTOs;

public class ReviewResponseDto
{
    public Guid Id { get; set; }
    public Guid ReviewerUserId { get; set; }
    public Guid ReviewedDriverId { get; set; }
    public Guid DeliveryId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
