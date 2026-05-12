using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Wasel.Api.Shared.Security;
using Xunit;

namespace Wasel.Api.Tests.Unit.Security;

public class CurrentUserServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly CurrentUserService _sut;

    public CurrentUserServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _sut = new CurrentUserService(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void Properties_WhenClaimsExist_ReturnCorrectValues()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "kc-123"),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.GivenName, "John"),
            new Claim(ClaimTypes.Surname, "Doe"),
            new Claim(ClaimTypes.Role, "ADMIN"),
            new Claim(ClaimTypes.Role, "CLIENT")
        };
        
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act & Assert
        _sut.KeycloakId.Should().Be("kc-123");
        _sut.Email.Should().Be("test@test.com");
        _sut.FirstName.Should().Be("John");
        _sut.LastName.Should().Be("Doe");
        _sut.Roles.Should().BeEquivalentTo(new[] { "ADMIN", "CLIENT" });
        _sut.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void Properties_WhenClaimsAreMissing_ReturnNullOrEmpty()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // No auth type = not authenticated
        var principal = new ClaimsPrincipal(identity);
        
        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        // Act & Assert
        _sut.KeycloakId.Should().BeNull();
        _sut.Email.Should().BeNull();
        _sut.FirstName.Should().BeNull();
        _sut.LastName.Should().BeNull();
        _sut.Roles.Should().BeEmpty();
        _sut.IsAuthenticated.Should().BeFalse();
    }
}
