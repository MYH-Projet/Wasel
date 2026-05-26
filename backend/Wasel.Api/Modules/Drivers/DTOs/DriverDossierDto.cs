using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Modules.Documents.DTOs;

namespace Wasel.Api.Modules.Drivers.DTOs;

public class DriverDossierDto
{
    public Guid DriverId { get; set; }
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Cin { get; set; } = string.Empty;

    public string PermitNumber { get; set; } = string.Empty;
    public DriverStatus DriverStatus { get; set; }

    public Guid? DossierId { get; set; }
    public DriverDossierStatus? DossierStatus { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? RejectionReason { get; set; }

    public List<DocumentResponseDto> Documents { get; set; } = new();
}