using Wasel.Api.Modules.Drivers.Enums;

namespace Wasel.Api.Modules.Drivers.DTOs;

public class DriverMeResponseDto
{
    public Guid DriverId { get; set; }
    public Guid UserId { get; set; }
    public string PermitNumber { get; set; } = string.Empty;
    public DriverStatus DriverStatus { get; set; }
    public DriverDossierStatus DossierStatus { get; set; }
    public DateTime? SubmissionDate { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? RejectionReason { get; set; }
    public VehicleResponseDto? Vehicle { get; set; }
    public DateTime CreatedAt { get; set; }
}
