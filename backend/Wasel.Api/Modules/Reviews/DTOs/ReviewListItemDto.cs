namespace Wasel.Api.Modules.Reviews.DTOs;

public class ReviewListItemDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime Date { get; set; }
    public string ReviewerFirstName { get; set; } = string.Empty;
}
