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

    [Fact]
    public async Task UpdateMyProfileAsync_ValidPhone_UpdatesAllowedFields()
    {
        // Arrange
        var keycloakId = "kc-123";
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            FirstName = "OldName",
            LastName = "OldLastName",
            Phone = "0600000000",
            ProfileObjectKey = "old-key"
        };

        _userRepositoryMock.Setup(x => x.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(existingUser);

        var request = new Modules.Users.DTOs.UpdateMyProfileRequestDto
        {
            FirstName = "NewName",
            LastName = "NewLastName",
            Phone = "0611111111",
            ProfileObjectKey = "new-key"
        };

        // Act
        var result = await _sut.UpdateMyProfileAsync(keycloakId, request);

        // Assert
        result.FirstName.Should().Be("NewName");
        result.LastName.Should().Be("NewLastName");
        result.Phone.Should().Be("0611111111");
        result.ProfileObjectKey.Should().Be("new-key");

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => 
            u.FirstName == "NewName" && 
            u.Phone == "0611111111")), Times.Once);
    }

    [Fact]
    public async Task UpdateMyProfileAsync_PartialBody_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var keycloakId = "kc-123";
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            FirstName = "OldName",
            LastName = "OldLastName",
            Phone = "0600000000"
        };

        _userRepositoryMock.Setup(x => x.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(existingUser);

        var request = new Modules.Users.DTOs.UpdateMyProfileRequestDto
        {
            FirstName = "NewName"
            // LastName, Phone, ProfileObjectKey are null
        };

        // Act
        var result = await _sut.UpdateMyProfileAsync(keycloakId, request);

        // Assert
        result.FirstName.Should().Be("NewName");
        result.LastName.Should().Be("OldLastName"); // Should remain unchanged
        result.Phone.Should().Be("0600000000"); // Should remain unchanged

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMyPreferencesAsync_ClientMode_CreatesOrUpdatesPreferences()
    {
        // Arrange
        var keycloakId = "kc-123";
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            Preference = null
        };

        _userRepositoryMock.Setup(x => x.GetUserWithPreferenceAndDriverAsync(keycloakId)).ReturnsAsync(existingUser);

        var request = new Modules.Users.DTOs.UpdateUserPreferencesRequestDto
        {
            ActiveAppMode = ActiveAppMode.CLIENT,
            PreferredMode = ActiveAppMode.CLIENT
        };

        // Act
        var result = await _sut.UpdateMyPreferencesAsync(keycloakId, request);

        // Assert
        result.ActiveAppMode.Should().Be(ActiveAppMode.CLIENT);
        result.PreferredMode.Should().Be(ActiveAppMode.CLIENT);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.Is<User>(u => 
            u.Preference != null && 
            u.Preference.ActiveAppMode == ActiveAppMode.CLIENT)), Times.Once);
    }

    [Fact]
    public async Task UpdateMyPreferencesAsync_DriverMode_WithoutDriverProfile_ThrowsException()
    {
        // Arrange
        var keycloakId = "kc-123";
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            Driver = null // No driver profile
        };

        _userRepositoryMock.Setup(x => x.GetUserWithPreferenceAndDriverAsync(keycloakId)).ReturnsAsync(existingUser);

        var request = new Modules.Users.DTOs.UpdateUserPreferencesRequestDto
        {
            ActiveAppMode = ActiveAppMode.DRIVER,
            PreferredMode = ActiveAppMode.CLIENT
        };

        // Act & Assert
        await Assert.ThrowsAsync<Shared.Exceptions.ApiException>(() => _sut.UpdateMyPreferencesAsync(keycloakId, request));
    }

    [Fact]
    public async Task UpdateMyPreferencesAsync_DriverMode_WithDriverProfile_Succeeds()
    {
        // Arrange
        var keycloakId = "kc-123";
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            KeycloakId = keycloakId,
            Driver = new Modules.Drivers.Entities.Driver(), // Driver profile exists
            Preference = new UserPreference
            {
                ActiveAppMode = ActiveAppMode.CLIENT,
                PreferredMode = ActiveAppMode.CLIENT
            }
        };

        _userRepositoryMock.Setup(x => x.GetUserWithPreferenceAndDriverAsync(keycloakId)).ReturnsAsync(existingUser);

        var request = new Modules.Users.DTOs.UpdateUserPreferencesRequestDto
        {
            ActiveAppMode = ActiveAppMode.DRIVER,
            PreferredMode = ActiveAppMode.DRIVER
        };

        // Act
        var result = await _sut.UpdateMyPreferencesAsync(keycloakId, request);

        // Assert
        result.ActiveAppMode.Should().Be(ActiveAppMode.DRIVER);
        result.PreferredMode.Should().Be(ActiveAppMode.DRIVER);

        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }
}
