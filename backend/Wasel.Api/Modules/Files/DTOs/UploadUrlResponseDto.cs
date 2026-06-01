namespace Wasel.Api.Modules.Files.DTOs;

public class UploadUrlResponseDto
{
    public string UploadUrl { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public int ExpiresInSeconds { get; set; }
}
