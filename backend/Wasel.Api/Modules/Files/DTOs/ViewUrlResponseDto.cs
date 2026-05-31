namespace Wasel.Api.Modules.Files.DTOs;

public class ViewUrlResponseDto
{
    public string ViewUrl { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
}
