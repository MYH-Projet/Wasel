using Wasel.Api.Modules.Documents.Enums;

namespace Wasel.Api.Modules.Documents.DTOs;

public class DriverDocumentResponseDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}
