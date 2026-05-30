using Wasel.Api.Modules.Documents.Enums;

namespace Wasel.Api.Modules.Documents.DTOs;

public class DocumentResponseDto
{
    public Guid DocumentId { get; set; }

    public DocumentType DocumentType { get; set; }

    public string ObjectKey { get; set; } = string.Empty;

    public DocumentStatus Status { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }
}