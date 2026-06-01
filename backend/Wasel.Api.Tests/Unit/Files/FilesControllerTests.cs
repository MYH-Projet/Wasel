using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Wasel.Api.Infrastructure.Keycloak;
using Wasel.Api.Infrastructure.MinIO;
using Wasel.Api.Modules.Files.Controllers;
using Wasel.Api.Modules.Files.DTOs;
using Wasel.Api.Modules.Files.Enums;
using Wasel.Api.Shared.Security;

namespace Wasel.Api.Tests.Unit.Files;

public class FilesControllerTests
{
    [Fact]
    public async Task CreateUploadUrl_ValidRequest_ReturnsPresignedUrlAndObjectKey()
    {
        var storage = new Mock<IStorageService>();
        storage.Setup(s => s.GenerateUploadUrlAsync(
                It.IsAny<string>(),
                "application/pdf",
                TimeSpan.FromSeconds(600)))
            .ReturnsAsync("https://minio/upload");

        var controller = CreateController(storage.Object);

        var result = await controller.CreateUploadUrl(new CreateUploadUrlRequestDto
        {
            FileName = "permis.pdf",
            FileType = "pdf",
            Context = FileContext.DOCUMENT
        });

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<UploadUrlResponseDto>().Subject;

        response.UploadUrl.Should().Be("https://minio/upload");
        response.ObjectKey.Should().StartWith("documents/kc-user-1/");
        response.ObjectKey.Should().EndWith(".pdf");
        response.ExpiresInSeconds.Should().Be(600);
    }

    [Fact]
    public async Task CreateUploadUrl_InvalidFileType_ReturnsBadRequest()
    {
        var controller = CreateController();

        var result = await controller.CreateUploadUrl(new CreateUploadUrlRequestDto
        {
            FileName = "script.exe",
            FileType = "exe",
            Context = FileContext.DOCUMENT
        });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateViewUrl_OwnerObjectKey_ReturnsPresignedUrl()
    {
        var storage = new Mock<IStorageService>();
        storage.Setup(s => s.GenerateViewUrlAsync(
                "documents/kc-user-1/file.pdf",
                TimeSpan.FromSeconds(300)))
            .ReturnsAsync("https://minio/view");

        var controller = CreateController(storage.Object);

        var result = await controller.CreateViewUrl("documents/kc-user-1/file.pdf");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ViewUrlResponseDto>().Subject;

        response.ViewUrl.Should().Be("https://minio/view");
        response.ExpiresInSeconds.Should().Be(300);
    }

    [Fact]
    public async Task CreateViewUrl_NonOwnerNonAdmin_ReturnsForbidden()
    {
        var controller = CreateController();

        var result = await controller.CreateViewUrl("documents/other-user/file.pdf");

        var status = result.Should().BeOfType<ObjectResult>().Subject;
        status.StatusCode.Should().Be(403);
    }

    [Fact]
    public async Task CreateViewUrl_Admin_ReturnsPresignedUrlForAnyObjectKey()
    {
        var storage = new Mock<IStorageService>();
        storage.Setup(s => s.GenerateViewUrlAsync(
                "documents/other-user/file.pdf",
                TimeSpan.FromSeconds(300)))
            .ReturnsAsync("https://minio/admin-view");

        var controller = CreateController(
            storage.Object,
            roles: new[] { KeycloakConstants.RoleAdmin });

        var result = await controller.CreateViewUrl("documents/other-user/file.pdf");

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ViewUrlResponseDto>().Subject;
        response.ViewUrl.Should().Be("https://minio/admin-view");
    }

    private static FilesController CreateController(
        IStorageService? storageService = null,
        IReadOnlyList<string>? roles = null)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(c => c.IsAuthenticated).Returns(true);
        currentUser.SetupGet(c => c.KeycloakId).Returns("kc-user-1");
        currentUser.SetupGet(c => c.Roles).Returns(roles ?? new[] { "CLIENT" });

        return new FilesController(
            storageService ?? Mock.Of<IStorageService>(),
            currentUser.Object);
    }
}
