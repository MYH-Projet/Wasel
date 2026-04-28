using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Drivers.Entities;

public class Driver : BaseEntity
{
    public Guid UserId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public DriverStatus Status { get; set; } = DriverStatus.Pending;
    public bool IsAvailable { get; set; } = false;
}
