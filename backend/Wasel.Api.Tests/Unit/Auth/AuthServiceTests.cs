using FluentAssertions;
using Moq;
using Wasel.Api.Modules.Auth.Services;
using Wasel.Api.Modules.Users.DTOs;
using Wasel.Api.Modules.Users.Services;
using Wasel.Api.Shared.Exceptions;
using Wasel.Api.Shared.Security;
using Xunit;

namespace Wasel.Api.Tests.Unit.Auth;

public class AuthServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUserService> _userServiceMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _userServiceMock = new Mock<IUserService>();
        _sut = new AuthService(_currentUserServiceMock.Object, _userServiceMock.Object);
    }

    [Fact]
    public async Task EnsureCurrentUserExistsAsync_WhenUserExists_ReturnsDto()
    {
        // Arrange
        var keycloakId = "kc-123";
        var email = "test@test.com";
        var localUserId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.KeycloakId).Returns(keycloakId);
        _currentUserServiceMock.Setup(x => x.Email).Returns(email);
        _currentUserServiceMock.Setup(x => x.FirstName).Returns("John");
        _currentUserServiceMock.Setup(x => x.LastName).Returns("Doe");
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string> { "ADMIN" });

        var userResponse = new UserResponseDto
        {
            Id = localUserId,
            KeycloakId = keycloakId,
            Email = email,
            FirstName = "John",
            LastName = "Doe"
        };

        _userServiceMock
            // ✅ ADDED 5TH PARAMETER: It.IsAny<List<string>>()
            .Setup(x => x.FindOrCreateFromKeycloakAsync(keycloakId, email, "John", "Doe", It.IsAny<List<string>>()))
            .ReturnsAsync(userResponse);

        // Act
        var result = await _sut.EnsureCurrentUserExistsAsync();

        // Assert
        result.LocalUserId.Should().Be(localUserId);
        result.KeycloakId.Should().Be(keycloakId);
        result.Email.Should().Be(email);
        result.Roles.Should().Contain("ADMIN");
        
        // ✅ ADDED 5TH PARAMETER: It.IsAny<List<string>>()
        _userServiceMock.Verify(x => x.FindOrCreateFromKeycloakAsync(keycloakId, email, "John", "Doe", It.IsAny<List<string>>()), Times.Once);
    }

    [Fact]
    public async Task EnsureCurrentUserExistsAsync_WhenKeycloakIdIsMissing_ThrowsUnauthorized()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.KeycloakId).Returns((string?)null);
        _currentUserServiceMock.Setup(x => x.Email).Returns("test@test.com");
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => _sut.EnsureCurrentUserExistsAsync());
        exception.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task EnsureCurrentUserExistsAsync_WhenEmailIsMissing_ThrowsUnauthorized()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.KeycloakId).Returns("kc-123");
        _currentUserServiceMock.Setup(x => x.Email).Returns((string?)null);
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string>());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() => _sut.EnsureCurrentUserExistsAsync());
        exception.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task EnsureCurrentUserExistsAsync_WhenDbErrorOccurs_ThrowsExceptionUnmasked()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.KeycloakId).Returns("kc-123");
        _currentUserServiceMock.Setup(x => x.Email).Returns("test@test.com");
        _currentUserServiceMock.Setup(x => x.Roles).Returns(new List<string>()); 

        _userServiceMock
            // ✅ ADDED 5TH PARAMETER: It.IsAny<List<string>>()
            .Setup(x => x.FindOrCreateFromKeycloakAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<string>>()))
            .ThrowsAsync(new InvalidOperationException("DB Failure"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.EnsureCurrentUserExistsAsync());
    }
}