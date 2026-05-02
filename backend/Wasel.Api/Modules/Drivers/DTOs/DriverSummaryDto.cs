using Wasel.Api.Modules.Drivers.Enums;

namespace Wasel.Api.Modules.Drivers.DTOs;

public class DriverSummaryDto
{
    public Guid DriverId { get; set; }
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string PermitNumber { get; set; } = string.Empty;
    public DriverStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}

