using Moq;
using Wasel.Api.Modules.Drivers.DTOs;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Drivers.Enums;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Drivers.Services;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Shared.Exceptions;

namespace Wasel.Api.Tests.Unit.Drivers;

public class DriverServiceTests
{
    private readonly Mock<IDriverRepository> _mockDriverRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly DriverService _driverService;

    public DriverServiceTests()
    {
        _mockDriverRepository = new Mock<IDriverRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _driverService = new DriverService(_mockDriverRepository.Object, _mockUserRepository.Object);
    }

    [Fact]
    public async Task RegisterCurrentUserAsDriverAsync_CreatesDriverDossierAndVehicle()
    {
        // Arrange
        var keycloakId = "kc-123";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, KeycloakId = keycloakId };
        var request = new RegisterDriverRequestDto
        {
            PermitNumber = "B123456",
            Vehicle = new VehicleRequestDto
            {
                Type = "MOTORCYCLE",
                Matricule = "12345-A-6",
                Marque = "Honda",
                Model = "Click 125"
            }
        };

        var createdDriver = new Driver
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PermitNumber = "B123456",
            Status = DriverStatus.PendingVerification,
            CreatedAt = DateTime.UtcNow,
            Dossier = new DriverDossier { Status = DriverDossierStatus.Draft },
            Vehicle = new Vehicle { Type = "MOTORCYCLE", Matricule = "12345-A-6", Marque = "Honda", Model = "Click 125" }
        };

        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.ExistsByUserIdAsync(userId)).ReturnsAsync(false);
        _mockDriverRepository.Setup(r => r.ExistsByPermitNumberAsync(request.PermitNumber)).ReturnsAsync(false);
        _mockDriverRepository.Setup(r => r.GetByUserIdWithDossierAndVehicleAsync(userId)).ReturnsAsync(createdDriver);

        // Act
        var result = await _driverService.RegisterCurrentUserAsDriverAsync(keycloakId, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("B123456", result.PermitNumber);
        Assert.Equal(DriverStatus.PendingVerification, result.DriverStatus);
        Assert.Equal(DriverDossierStatus.Draft, result.DossierStatus);
        Assert.NotNull(result.Vehicle);
        Assert.Equal("MOTORCYCLE", result.Vehicle.Type);

        _mockDriverRepository.Verify(r => r.AddAsync(It.IsAny<Driver>()), Times.Once);
        _mockDriverRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterCurrentUserAsDriverAsync_WhenAlreadyDriver_ThrowsConflict()
    {
        // Arrange
        var keycloakId = "kc-123";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, KeycloakId = keycloakId };
        var request = new RegisterDriverRequestDto { PermitNumber = "B123456" };

        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.ExistsByUserIdAsync(userId)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() =>
            _driverService.RegisterCurrentUserAsDriverAsync(keycloakId, request));

        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task RegisterCurrentUserAsDriverAsync_WhenPermitNumberUsed_ThrowsConflict()
    {
        // Arrange
        var keycloakId = "kc-123";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, KeycloakId = keycloakId };
        var request = new RegisterDriverRequestDto { PermitNumber = "B123456" };

        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.ExistsByUserIdAsync(userId)).ReturnsAsync(false);
        _mockDriverRepository.Setup(r => r.ExistsByPermitNumberAsync(request.PermitNumber)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() =>
            _driverService.RegisterCurrentUserAsDriverAsync(keycloakId, request));

        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task GetCurrentDriverProfileAsync_WhenDriverExists_ReturnsProfile()
    {
        // Arrange
        var keycloakId = "kc-123";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, KeycloakId = keycloakId };

        var existingDriver = new Driver
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PermitNumber = "B123456",
            Status = DriverStatus.Approved,
            CreatedAt = DateTime.UtcNow,
            Dossier = new DriverDossier { Status = DriverDossierStatus.Approved },
            Vehicle = new Vehicle { Type = "CAR" }
        };

        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.GetByUserIdWithDossierAndVehicleAsync(userId)).ReturnsAsync(existingDriver);

        // Act
        var result = await _driverService.GetCurrentDriverProfileAsync(keycloakId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DriverStatus.Approved, result.DriverStatus);
        Assert.Equal(DriverDossierStatus.Approved, result.DossierStatus);
        Assert.Equal("CAR", result.Vehicle!.Type);
    }

    [Fact]
    public async Task GetCurrentDriverProfileAsync_WhenNoDriver_ThrowsNotFound()
    {
        // Arrange
        var keycloakId = "kc-123";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, KeycloakId = keycloakId };

        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.GetByUserIdWithDossierAndVehicleAsync(userId)).ReturnsAsync((Driver?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() =>
            _driverService.GetCurrentDriverProfileAsync(keycloakId));

        Assert.Equal(404, exception.StatusCode);
    }

    [Fact]
    public async Task SubmitCurrentDriverDossierAsync_WhenDraft_SetsSubmittedAndDate()
    {
        // Arrange
        var keycloakId = "kc-123";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, KeycloakId = keycloakId };

        var existingDriver = new Driver
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Dossier = new DriverDossier { Status = DriverDossierStatus.Draft }
        };

        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.GetByUserIdWithDossierAndVehicleAsync(userId)).ReturnsAsync(existingDriver);

        // Act
        var result = await _driverService.SubmitCurrentDriverDossierAsync(keycloakId);

        // Assert
        Assert.Equal(DriverDossierStatus.Submitted, result.DossierStatus);
        Assert.NotEqual(DateTime.MinValue, existingDriver.Dossier.SubmissionDate);
        _mockDriverRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task SubmitCurrentDriverDossierAsync_WhenNotDraft_ThrowsBadRequest()
    {
        // Arrange
        var keycloakId = "kc-123";
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, KeycloakId = keycloakId };

        var existingDriver = new Driver
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Dossier = new DriverDossier { Status = DriverDossierStatus.Submitted } // Already submitted
        };

        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync(keycloakId)).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.GetByUserIdWithDossierAndVehicleAsync(userId)).ReturnsAsync(existingDriver);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ApiException>(() =>
            _driverService.SubmitCurrentDriverDossierAsync(keycloakId));

        Assert.Equal(400, exception.StatusCode);
    }
}
