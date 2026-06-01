using System.ComponentModel.DataAnnotations;
using Wasel.Api.Modules.Users.Enums;

namespace Wasel.Api.Modules.Users.DTOs;

public class UpdateUserPreferencesRequestDto
{
    [Required]
    public ActiveAppMode ActiveAppMode { get; set; }

    [Required]
    public ActiveAppMode PreferredMode { get; set; }
}
