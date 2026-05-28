using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Wasel.Api.Tests.Security;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme = "TestScheme";
    public static readonly string DefaultKeycloakId = "kc-test-user";
    public static readonly string DefaultEmail = "test@wasel.ma";
    public static readonly string DefaultRole = "CLIENT";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // On lit les claims envoyés dans les headers par le client de test
        // S'il n'y en a pas, on met des valeurs par défaut
        var claims = new List<Claim>();

        if (Context.Request.Headers.TryGetValue("X-Test-KeycloakId", out var keycloakId))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, keycloakId.ToString()));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, DefaultKeycloakId));
        }

        if (Context.Request.Headers.TryGetValue("X-Test-Email", out var email))
        {
            claims.Add(new Claim(ClaimTypes.Email, email.ToString()));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Email, DefaultEmail));
        }

        if (Context.Request.Headers.TryGetValue("X-Test-Role", out var role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }
        else
        {
            claims.Add(new Claim(ClaimTypes.Role, DefaultRole));
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}
