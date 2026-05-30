using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wasel.Api.Modules.Users.DTOs;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Shared.Database;
using Wasel.Api.Tests.Fixtures;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wasel.Api.Tests;

public class UsersEndpointsTests : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly HttpClient _client;
    private readonly IntegrationTestWebAppFactory _factory;

    public UsersEndpointsTests(IntegrationTestWebAppFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task<User> SeedUserAsync(User user)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task GetAllUsers_AdminRole_ReturnsUsers()
    {
        // Arrange
        await SeedUserAsync(new User
{
    KeycloakId = "kc-user-1",
    Email = "user1@wasel.ma",
    FirstName = "Test",
    LastName = "User1",
    Preference = new UserPreference
    {
        ActiveAppMode = ActiveAppMode.CLIENT
    }
});

        // Act: Envoi de la requête en simulant un administrateur
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/users");
        request.Headers.Add("X-Test-Role", "ADMIN");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        options.Converters.Add(new JsonStringEnumConverter());

        var result = await response.Content.ReadFromJsonAsync<PaginatedResponseDto<UserResponseDto>>(options);

result.Should().NotBeNull();
result!.Items.Should().Contain(u => u.Email == "user1@wasel.ma");
result.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllUsers_NonAdminRole_ReturnsForbidden()
    {
        // Act: Simulation d'un client (pas ADMIN)
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/users");
        request.Headers.Add("X-Test-Role", "CLIENT");
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangeUserStatus_AdminRole_UpdatesStatus()
    {
        // Arrange
        var user = await SeedUserAsync(new User 
        { 
            KeycloakId = "kc-status-1", 
            Email = "status1@wasel.ma",
            FirstName = "Test",
            LastName = "Status1",
            Status = UserStatus.Pending
        });

        var changeRequest = new ChangeUserStatusRequestDto
        {
            Status = UserStatus.Active
        };

        var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/admin/users/{user.Id}/status");
        request.Headers.Add("X-Test-Role", "ADMIN");
        request.Content = JsonContent.Create(changeRequest);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WaselDbContext>();
        var updatedUser = db.Users.Find(user.Id);
        updatedUser!.Status.Should().Be(UserStatus.Active);
    }
}
