namespace Wasel.Api.Modules.Drivers.DTOs;

public class AdminDriverListItemDto
{
    public Guid DriverId { get; set; }
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Cin { get; set; } = string.Empty;

    public string PermitNumber { get; set; } = string.Empty;

    public string DriverStatus { get; set; } = string.Empty;
    public string? DossierStatus { get; set; }

    public DateTime CreatedAt { get; set; }
}