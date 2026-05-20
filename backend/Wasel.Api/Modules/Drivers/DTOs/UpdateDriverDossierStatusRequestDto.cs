namespace Wasel.Api.Modules.Drivers.DTOs;

public class UpdateDriverDossierStatusRequestDto
{
    public string Status { get; set; } = string.Empty;

    public string? RejectionReason { get; set; }
}