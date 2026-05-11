using System.Security.Claims;
using FluentAssertions;
using Wasel.Api.Infrastructure.Keycloak;
using Wasel.Api.Shared.Security;
using Xunit;

namespace Wasel.Api.Tests.Unit.Security;

public class KeycloakClaimsTransformerTests
{
    private readonly KeycloakClaimsTransformer _sut;

    public KeycloakClaimsTransformerTests()
    {
        _sut = new KeycloakClaimsTransformer();
    }

    [Fact]
    public async Task TransformAsync_ExtractsValidRolesAndIgnoresTechnicalRoles()
    {
        // Arrange
        var realmAccessJson = "{\"roles\":[\"ADMIN\",\"CLIENT\",\"DRIVER\",\"offline_access\",\"uma_authorization\",\"default-roles-wasel\"]}";
        
        var identity = new ClaimsIdentity("TestAuthType");
        identity.AddClaim(new Claim(KeycloakConstants.RealmAccessClaim, realmAccessJson));
        
        var principal = new ClaimsPrincipal(identity);

        // Act
        var transformedPrincipal = await _sut.TransformAsync(principal);

        // Assert
        var roleClaims = transformedPrincipal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        
        roleClaims.Should().Contain("ADMIN");
        roleClaims.Should().Contain("CLIENT");
        roleClaims.Should().Contain("DRIVER");
        
        roleClaims.Should().NotContain("offline_access");
        roleClaims.Should().NotContain("uma_authorization");
        roleClaims.Should().NotContain("default-roles-wasel");
        
        roleClaims.Should().HaveCount(3);
    }
    
    [Fact]
    public async Task TransformAsync_WhenNoRealmAccess_ReturnsUnmodified()
    {
        // Arrange
        var identity = new ClaimsIdentity("TestAuthType");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var transformedPrincipal = await _sut.TransformAsync(principal);

        // Assert
        transformedPrincipal.HasClaim(c => c.Type == ClaimTypes.Role).Should().BeFalse();
    }
}
