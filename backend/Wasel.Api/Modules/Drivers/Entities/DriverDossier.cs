using Wasel.Api.Shared.Common;
using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Modules.Documents.Entities;

namespace Wasel.Api.Modules.Drivers.Entities;

public class DriverDossier : BaseEntity
{
    public Guid DriverId { get; set; }

    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

    public DateTime? VerificationDate { get; set; }

    public DriverDossierStatus Status { get; set; } = DriverDossierStatus.Draft;

    public string? RejectionReason { get; set; }

    public Driver Driver { get; set; } = null!;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}