using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Notifications.Entities;

public class UserDeviceToken : BaseEntity
{
    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public string Platform { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime? LastUsedAt { get; set; }

    // Navigation
    public User User { get; set; } = default!;
}
