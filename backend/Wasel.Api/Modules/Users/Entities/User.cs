using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Shared.Common;
using Wasel.Api.Modules.Drivers.Entities;
namespace Wasel.Api.Modules.Users.Entities;

public class User : BaseEntity
{
    public string KeycloakId { get; set; } = string.Empty;

    public string Cin { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public UserStatus Status { get; set; } = UserStatus.Active;

    public string? ProfileObjectKey { get; set; }

    public Driver? Driver { get; set; }

    public UserPreference? Preference { get; set; }
}