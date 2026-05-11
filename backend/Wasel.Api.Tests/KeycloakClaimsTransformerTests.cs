using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Wasel.Api.Infrastructure.Keycloak;
using Wasel.Api.Shared.Security;

namespace Wasel.Api.Tests;

/// <summary>
/// Tests for KeycloakClaimsTransformer.
/// Verifies that Keycloak realm_access roles are correctly mapped to ClaimTypes.Role.
/// No Keycloak server needed — we build ClaimsPrincipal objects manually.
/// </summary>
public class KeycloakClaimsTransformerTests
{
    private readonly KeycloakClaimsTransformer _transformer = new();

    /// <summary>
    /// Helper: creates a ClaimsPrincipal with a realm_access claim containing the given roles.
    /// </summary>
    private static ClaimsPrincipal CreatePrincipalWithRoles(params string[] roles)
    {
        var realmAccess = JsonSerializer.Serialize(new { roles });

        var claims = new List<Claim>
        {
            new(KeycloakConstants.RealmAccessClaim, realmAccess)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Helper: creates a ClaimsPrincipal with no realm_access claim at all.
    /// </summary>
    private static ClaimsPrincipal CreatePrincipalWithoutRealmAccess()
    {
        var identity = new ClaimsIdentity(new List<Claim>(), "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task Transform_KnownRoles_AddedAsClaimTypesRole()
    {
        // Arrange: principal with ADMIN, DRIVER, CLIENT
        var principal = CreatePrincipalWithRoles("ADMIN", "DRIVER", "CLIENT");

        // Act
        var result = await _transformer.TransformAsync(principal);

        // Assert
        var roleClaims = result.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        roleClaims.Should().Contain("ADMIN");
        roleClaims.Should().Contain("DRIVER");
        roleClaims.Should().Contain("CLIENT");
        roleClaims.Should().HaveCount(3);
    }

    [Fact]
    public async Task Transform_UnknownRoles_AreIgnored()
    {
        // Arrange: principal with known + unknown roles
        var principal = CreatePrincipalWithRoles("ADMIN", "uma_authorization", "offline_access", "default-roles-wasel");

        // Act
        var result = await _transformer.TransformAsync(principal);

        // Assert: only ADMIN is mapped
        var roleClaims = result.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        roleClaims.Should().ContainSingle()
            .Which.Should().Be("ADMIN");
    }

    [Fact]
    public async Task Transform_CalledTwice_DoesNotDuplicateRoles()
    {
        // Arrange
        var principal = CreatePrincipalWithRoles("ADMIN", "CLIENT");

        // Act: transform twice
        var firstResult = await _transformer.TransformAsync(principal);
        var secondResult = await _transformer.TransformAsync(firstResult);

        // Assert: roles are NOT duplicated
        var roleClaims = secondResult.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        roleClaims.Should().HaveCount(2);
    }

    [Fact]
    public async Task Transform_NoRealmAccessClaim_NoRolesAdded()
    {
        // Arrange: principal without realm_access
        var principal = CreatePrincipalWithoutRealmAccess();

        // Act
        var result = await _transformer.TransformAsync(principal);

        // Assert: no role claims
        var roleClaims = result.FindAll(ClaimTypes.Role).ToList();
        roleClaims.Should().BeEmpty();
    }
}
