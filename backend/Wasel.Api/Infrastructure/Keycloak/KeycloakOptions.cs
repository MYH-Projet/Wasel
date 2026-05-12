namespace Wasel.Api.Infrastructure.Keycloak;

public class KeycloakOptions
{
    public const string SectionName = "Keycloak";
    
    public string Authority { get; set; } = string.Empty;
    public string InternalAuthority { get; set; } = string.Empty;
    public string NginxAuthority { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public bool RequireHttpsMetadata { get; set; }
}
