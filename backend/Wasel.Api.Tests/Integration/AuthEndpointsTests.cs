using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wasel.Api.Modules.Auth.DTOs;
using Wasel.Api.Shared.Database;
using Wasel.Api.Tests.Fixtures;

namespace Wasel.Api.Tests;

public class AuthEndpointsTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public AuthEndpointsTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetCurrentUser_WhenCalledFirstTime_CreatesUserAndReturnsProfile()
    {
        // Act: Appel à l'API sans utilisateur en base (mais avec le TestAuthHandler simulant un JWT valide)
        var response = await _client.GetAsync("/api/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var profile = await response.Content.ReadFromJsonAsync<CurrentUserResponseDto>();
        profile.Should().NotBeNull();
        profile!.Email.Should().Be("test@wasel.ma");
        profile.KeycloakId.Should().Be("kc-test-user");
        profile.Status.Should().Be("Active");

        // Vérification de la base de données réelle dans le Testcontainer
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
        var userInDb = db.Users.FirstOrDefault(u => u.KeycloakId == "kc-test-user");
        userInDb.Should().NotBeNull();
        userInDb!.Email.Should().Be("test@wasel.ma");
    }

    [Fact]
    public async Task UpdateProfile_WithValidData_UpdatesUserAndReturnsProfile()
    {
        // Arrange
        // Appel d'abord pour créer l'utilisateur
        await _client.GetAsync("/api/auth/me");

        var updateRequest = new UpdateCurrentUserProfileRequestDto
        {
            Cin = "AB123456",
            Phone = "+212600000000"
        };

        // Act
        var response = await _client.PatchAsJsonAsync("/api/auth/me/profile", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var profile = await response.Content.ReadFromJsonAsync<CurrentUserResponseDto>();
        profile.Should().NotBeNull();
        profile!.Cin.Should().Be("AB123456");
        profile.Phone.Should().Be("+212600000000");

        // Vérification en base
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
        var userInDb = db.Users.FirstOrDefault(u => u.KeycloakId == "kc-test-user");
        userInDb!.Cin.Should().Be("AB123456");
        userInDb.Phone.Should().Be("+212600000000");
    }
}
