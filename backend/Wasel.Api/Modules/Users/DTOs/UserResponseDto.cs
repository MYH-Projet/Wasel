using Wasel.Api.Modules.Users.Enums;

namespace Wasel.Api.Modules.Users.DTOs;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string KeycloakId { get; set; } = string.Empty;
    public string Cin { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public string? ProfileObjectKey { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}