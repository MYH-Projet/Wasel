using FluentAssertions;
using Moq;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Enums;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Modules.Users.Services;
using Xunit;

namespace Wasel.Api.Tests.Unit.Users;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _sut = new UserService(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task FindOrCreateFromKeycloakAsync_WhenUserExistsByKeycloakId_ReturnsUserAndUpdatesDetailsIfChanged()
    {
        // Arrange
        var keycloakId = "kc-123";
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            Email = "old@test.com",
            FirstName = "OldName",
            LastName = "OldLastName"
        };

        _userRepositoryMock.Setup(x => x.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(existingUser);

        // Act
        var result = await _sut.FindOrCreateFromKeycloakAsync(keycloakId, "new@test.com", "NewName", "NewLastName");

        // Assert
        result.Email.Should().Be("new@test.com");
        result.FirstName.Should().Be("NewName");
        result.LastName.Should().Be("NewLastName");
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => 
            u.Email == "new@test.com" && 
            u.FirstName == "NewName" && 
            u.LastName == "NewLastName")), Times.Once);
    }

    [Fact]
    public async Task FindOrCreateFromKeycloakAsync_WhenUserExistsByEmailOnly_UpdatesKeycloakIdAndReturnsUser()
    {
        // Arrange
        var keycloakId = "kc-123";
        var email = "test@test.com";
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            KeycloakId = null, // Missing keycloak ID
            Email = email,
            FirstName = "Name",
            LastName = "LastName"
        };

        _userRepositoryMock.Setup(x => x.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(existingUser);

        // Act
        var result = await _sut.FindOrCreateFromKeycloakAsync(keycloakId, email, "NewName", "NewLastName");

        // Assert
        result.KeycloakId.Should().Be(keycloakId);
        result.FirstName.Should().Be("NewName");
        
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => u.KeycloakId == keycloakId)), Times.Once);
    }

    [Fact]
    public async Task FindOrCreateFromKeycloakAsync_WhenUserDoesNotExist_CreatesAndReturnsNewUser()
    {
        // Arrange
        var keycloakId = "kc-new";
        var email = "new@test.com";

        _userRepositoryMock.Setup(x => x.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync((User?)null);
        _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync((User?)null);

        // Act
        var result = await _sut.FindOrCreateFromKeycloakAsync(keycloakId, email, "NewName", "NewLastName");

        // Assert
        result.KeycloakId.Should().Be(keycloakId);
        result.Email.Should().Be(email);
        result.Status.Should().Be(UserStatus.Pending); // Status should be Pending as per logic
        
        _userRepositoryMock.Verify(x => x.AddAsync(It.Is<User>(u => 
            u.KeycloakId == keycloakId && 
            u.Email == email &&
            u.Status == UserStatus.Pending)), Times.Once);
    }
}
