using Moq;
using Wasel.Api.Modules.Documents.DTOs;
using Wasel.Api.Modules.Documents.Entities;
using Wasel.Api.Modules.Documents.Enums;
using Wasel.Api.Modules.Documents.Repositories;
using Wasel.Api.Modules.Documents.Services;
using Wasel.Api.Modules.Drivers.Entities;
using Wasel.Api.Modules.Drivers.Repositories;
using Wasel.Api.Modules.Users.Entities;
using Wasel.Api.Modules.Users.Repositories;
using Wasel.Api.Shared.Exceptions;
using Xunit;

namespace Wasel.Api.Tests.Unit.Documents;

public class DocumentServiceTests
{
    private readonly Mock<IDocumentRepository> _mockDocumentRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IDriverRepository> _mockDriverRepository;
    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        _mockDocumentRepository = new Mock<IDocumentRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockDriverRepository = new Mock<IDriverRepository>();

        _documentService = new DocumentService(
            _mockDocumentRepository.Object,
            _mockUserRepository.Object,
            _mockDriverRepository.Object);
    }

    [Fact]
    public async Task AddOrReplaceCurrentDriverDocumentAsync_WhenNoUser_ThrowsNotFound()
    {
        // Arrange
        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync("k-id"))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ApiException>(() =>
            _documentService.AddOrReplaceCurrentDriverDocumentAsync("k-id", new AddDriverDocumentRequestDto()));
    }

    [Fact]
    public async Task AddOrReplaceCurrentDriverDocumentAsync_WhenNoDriver_ThrowsNotFound()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync("k-id")).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.GetByUserIdWithDossierAndDocumentsAsync(user.Id)).ReturnsAsync((Driver?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            _documentService.AddOrReplaceCurrentDriverDocumentAsync("k-id", new AddDriverDocumentRequestDto()));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task AddOrReplaceCurrentDriverDocumentAsync_WhenNoDossier_ThrowsBadRequest()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var driver = new Driver { UserId = user.Id }; // No Dossier
        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync("k-id")).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.GetByUserIdWithDossierAndDocumentsAsync(user.Id)).ReturnsAsync(driver);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            _documentService.AddOrReplaceCurrentDriverDocumentAsync("k-id", new AddDriverDocumentRequestDto()));
        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task AddOrReplaceCurrentDriverDocumentAsync_NewDocument_CreatesPendingDocument()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid() };
        var driver = new Driver
        {
            UserId = user.Id,
            Dossier = new DriverDossier { Id = Guid.NewGuid(), Documents = new List<Document>() }
        };
        
        var request = new AddDriverDocumentRequestDto
        {
            DocumentType = DocumentType.Cin,
            ObjectKey = "test/path.pdf"
        };

        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync("k-id")).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.GetByUserIdWithDossierAndDocumentsAsync(user.Id)).ReturnsAsync(driver);

        // Act
        var result = await _documentService.AddOrReplaceCurrentDriverDocumentAsync("k-id", request);

        // Assert
        Assert.Equal("Cin", result.DocumentType);
        Assert.Equal("test/path.pdf", result.ObjectKey);
        Assert.Equal("Pending", result.Status);
        
        _mockDocumentRepository.Verify(r => r.AddAsync(It.Is<Document>(d => 
            d.ObjectKey == "test/path.pdf" && d.Status == DocumentStatus.Pending)), Times.Once);
        _mockDocumentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddOrReplaceCurrentDriverDocumentAsync_ExistingType_ReplacesObjectKeyAndResetsStatus()
    {
        // Arrange
        var existingDoc = new Document
        {
            Id = Guid.NewGuid(),
            DocumentType = DocumentType.Permit,
            ObjectKey = "old/path.pdf",
            Status = DocumentStatus.Rejected,
            RejectionReason = "Blurry"
        };
        
        var user = new User { Id = Guid.NewGuid() };
        var driver = new Driver
        {
            UserId = user.Id,
            Dossier = new DriverDossier { Id = Guid.NewGuid(), Documents = new List<Document> { existingDoc } }
        };
        
        var request = new AddDriverDocumentRequestDto
        {
            DocumentType = DocumentType.Permit,
            ObjectKey = "new/path.pdf"
        };

        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync("k-id")).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.GetByUserIdWithDossierAndDocumentsAsync(user.Id)).ReturnsAsync(driver);

        // Act
        var result = await _documentService.AddOrReplaceCurrentDriverDocumentAsync("k-id", request);

        // Assert
        Assert.Equal("Permit", result.DocumentType);
        Assert.Equal("new/path.pdf", result.ObjectKey);
        Assert.Equal("Pending", result.Status);
        Assert.Null(result.RejectionReason);
        
        Assert.Equal(DocumentStatus.Pending, existingDoc.Status);
        Assert.Null(existingDoc.RejectionReason);
        Assert.Equal("new/path.pdf", existingDoc.ObjectKey);
        
        _mockDocumentRepository.Verify(r => r.AddAsync(It.IsAny<Document>()), Times.Never);
        _mockDocumentRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetCurrentDriverDocumentsAsync_ReturnsOnlyCurrentDriverDocuments()
    {
        // Arrange
        var existingDoc = new Document
        {
            Id = Guid.NewGuid(),
            DocumentType = DocumentType.Cin,
            ObjectKey = "test/cin.pdf",
            Status = DocumentStatus.Approved
        };
        
        var user = new User { Id = Guid.NewGuid() };
        var driver = new Driver
        {
            UserId = user.Id,
            Dossier = new DriverDossier { Id = Guid.NewGuid(), Documents = new List<Document> { existingDoc } }
        };

        _mockUserRepository.Setup(r => r.GetByKeycloakIdAsync("k-id")).ReturnsAsync(user);
        _mockDriverRepository.Setup(r => r.GetByUserIdWithDossierAndDocumentsAsync(user.Id)).ReturnsAsync(driver);

        // Act
        var result = await _documentService.GetCurrentDriverDocumentsAsync("k-id");

        // Assert
        Assert.Single(result);
        var docDto = result.First();
        Assert.Equal(existingDoc.Id, docDto.Id);
        Assert.Equal("Cin", docDto.DocumentType);
        Assert.Equal("Approved", docDto.Status);
    }
}
