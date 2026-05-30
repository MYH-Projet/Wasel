using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Drivers.Entities;

public class Vehicle : BaseEntity
{
    public string Matricule { get; set; } = string.Empty;
    public string Marque { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // Navigation
    public Guid DriverId { get; set; }
    public Driver Driver { get; set; } = null!;
}




