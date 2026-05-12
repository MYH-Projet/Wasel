namespace Wasel.Api.Modules.Auth.DTOs;

public class CurrentUserResponseDto
{
    public string KeycloakId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    
    // Profil local (nullable si pas encore sync)
    public Guid? LocalUserId { get; set; }
    public string? Phone { get; set; }
    public string? Cin { get; set; }
    public string? Status { get; set; }
}
