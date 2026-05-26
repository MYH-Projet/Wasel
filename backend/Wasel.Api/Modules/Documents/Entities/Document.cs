using Wasel.Api.Shared.Common;
using Wasel.Api.Modules.Documents.Enums;
using Wasel.Api.Modules.Drivers.Entities;

namespace Wasel.Api.Modules.Documents.Entities;

public class Document : BaseEntity
{
    public Guid DriverDossierId { get; set; }

    public DocumentType DocumentType { get; set; }

    public string ObjectKey { get; set; } = string.Empty;

    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

    public string? RejectionReason { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public DateTime? VerifiedAt { get; set; }

    public DriverDossier DriverDossier { get; set; } = null!;
}