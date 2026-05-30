using System.ComponentModel.DataAnnotations;

namespace Wasel.Api.Modules.Users.DTOs;

public class UpdateMyProfileRequestDto
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    [RegularExpression(@"^(06|07|\+2126|\+2127)[0-9]{8}$", ErrorMessage = "Invalid phone number format.")]
    public string? Phone { get; set; }

    public string? ProfileObjectKey { get; set; }
}
