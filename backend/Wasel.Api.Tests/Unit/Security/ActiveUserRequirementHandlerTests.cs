using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Shared.Security;
using Xunit;

namespace Wasel.Api.Tests.Unit.Security;

public class ActiveUserRequirementHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly ActiveUserRequirementHandler _handler;

    public ActiveUserRequirementHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _handler = new ActiveUserRequirementHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsActive_Succeeds()
    {
        // Arrange
        var keycloakId = "keycloak-id-123";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, keycloakId) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { new ActiveUserRequirement() }, user, null);

        var dbUser = new User { Id = Guid.NewGuid(), KeycloakId = keycloakId, Status = UserStatus.Active };
        _userRepositoryMock.Setup(repo => repo.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(dbUser);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsBlocked_Fails()
    {
        // Arrange
        var keycloakId = "keycloak-id-123";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, keycloakId) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { new ActiveUserRequirement() }, user, null);

        var dbUser = new User { Id = Guid.NewGuid(), KeycloakId = keycloakId, Status = UserStatus.Blocked };
        _userRepositoryMock.Setup(repo => repo.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(dbUser);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsPending_Fails()
    {
        // Arrange
        var keycloakId = "keycloak-id-123";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, keycloakId) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { new ActiveUserRequirement() }, user, null);

        var dbUser = new User { Id = Guid.NewGuid(), KeycloakId = keycloakId, Status = UserStatus.Pending };
        _userRepositoryMock.Setup(repo => repo.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(dbUser);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserIsInactive_Fails()
    {
        // Arrange
        var keycloakId = "keycloak-id-123";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, keycloakId) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { new ActiveUserRequirement() }, user, null);

        var dbUser = new User { Id = Guid.NewGuid(), KeycloakId = keycloakId, Status = UserStatus.Inactive };
        _userRepositoryMock.Setup(repo => repo.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(dbUser);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserNotFound_Fails()
    {
        // Arrange
        var keycloakId = "keycloak-id-123";
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, keycloakId) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { new ActiveUserRequirement() }, user, null);

        _userRepositoryMock.Setup(repo => repo.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync((User?)null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenNoKeycloakId_Fails()
    {
        // Arrange
        var claims = Array.Empty<Claim>();
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);
        var context = new AuthorizationHandlerContext(new[] { new ActiveUserRequirement() }, user, null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
