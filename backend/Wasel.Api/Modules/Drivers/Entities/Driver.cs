using Wasel.Api.Shared.Common;
using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Modules.Users.Entities;

namespace Wasel.Api.Modules.Drivers.Entities;

public class Driver : BaseEntity
{
    public Guid UserId { get; set; }

    public string PermitNumber { get; set; } = string.Empty;

    public DriverStatus Status { get; set; } = DriverStatus.PendingVerification;

    public User User { get; set; } = null!;

    public DriverDossier? Dossier { get; set; }
}