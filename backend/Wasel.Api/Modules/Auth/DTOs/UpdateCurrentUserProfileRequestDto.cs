using System.ComponentModel.DataAnnotations;

namespace Wasel.Api.Modules.Auth.DTOs;

public class UpdateCurrentUserProfileRequestDto
{
    [MaxLength(20)]
    public string? Cin { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [MaxLength(255)]
    public string? ProfileObjectKey { get; set; }
}
