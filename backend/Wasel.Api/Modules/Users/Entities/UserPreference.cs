using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Shared.Common;

namespace Wasel.Api.Modules.Users.Entities;

public class UserPreference : BaseEntity
{
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public ActiveAppMode ActiveAppMode { get; set; } = ActiveAppMode.CLIENT;

    public ActiveAppMode PreferredMode { get; set; } = ActiveAppMode.CLIENT;
}