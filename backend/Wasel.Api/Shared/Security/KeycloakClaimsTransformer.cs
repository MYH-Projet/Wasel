using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Wasel.Api.Infrastructure.Keycloak;

namespace Wasel.Api.Shared.Security;

public class KeycloakClaimsTransformer : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var clone = principal.Clone();
        var identity = clone.Identity as ClaimsIdentity;

        if (identity == null)
        {
            return Task.FromResult(clone);
        }

        // Avoid adding claims multiple times if TransformAsync is called multiple times
        if (identity.HasClaim(c => c.Type == ClaimTypes.Role))
        {
            return Task.FromResult(clone);
        }

        var realmAccessClaim = clone.FindFirst(KeycloakConstants.RealmAccessClaim);

        if (realmAccessClaim != null)
        {
            var realmAccess = JsonSerializer.Deserialize<RealmAccess>(realmAccessClaim.Value);
            if (realmAccess?.Roles != null)
            {
                foreach (var role in realmAccess.Roles)
                {
                    // Map only our specific roles
                    if (role == KeycloakConstants.RoleAdmin || 
                        role == KeycloakConstants.RoleDriver || 
                        role == KeycloakConstants.RoleClient)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
            }
        }

        return Task.FromResult(clone);
    }

    private class RealmAccess
    {
        [System.Text.Json.Serialization.JsonPropertyName("roles")]
        public List<string> Roles { get; set; } = new();
    }
}
