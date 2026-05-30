using Wasel.Api.Modules.Users.Enums;

namespace Wasel.Api.Modules.Users.DTOs;

public class UserPreferencesResponseDto
{
    public ActiveAppMode ActiveAppMode { get; set; }
    public ActiveAppMode PreferredMode { get; set; }
}
