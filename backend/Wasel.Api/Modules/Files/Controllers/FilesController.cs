using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Infrastructure.Keycloak;
using Wasel.Api.Infrastructure.MinIO;
using Wasel.Api.Modules.Files.DTOs;
using Wasel.Api.Modules.Files.Enums;
using Wasel.Api.Shared.Security;

namespace Wasel.Api.Modules.Files.Controllers;

[ApiController]
[Route("api/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private const int UploadUrlExpirySeconds = 600;
    private const int ViewUrlExpirySeconds = 300;

    private static readonly IReadOnlyDictionary<string, string> AllowedContentTypes =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["jpg"] = "image/jpeg",
            ["jpeg"] = "image/jpeg",
            ["png"] = "image/png",
            ["pdf"] = "application/pdf"
        };

    private readonly IStorageService _storageService;
    private readonly ICurrentUserService _currentUserService;

    public FilesController(IStorageService storageService, ICurrentUserService currentUserService)
    {
        _storageService = storageService;
        _currentUserService = currentUserService;
    }

    [HttpPost("upload-url")]
    public async Task<IActionResult> CreateUploadUrl([FromBody] CreateUploadUrlRequestDto request)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.KeycloakId))
        {
            return Unauthorized(new { message = "Utilisateur non authentifie." });
        }

        var extension = NormalizeFileType(request.FileType);
        if (!AllowedContentTypes.TryGetValue(extension, out var contentType))
        {
            return BadRequest(new { message = "fileType invalide. Valeurs autorisees: jpg, jpeg, png, pdf." });
        }

        var objectKey = BuildObjectKey(request.Context, _currentUserService.KeycloakId, extension);
        var uploadUrl = await _storageService.GenerateUploadUrlAsync(
            objectKey,
            contentType,
            TimeSpan.FromSeconds(UploadUrlExpirySeconds));

        return Ok(new UploadUrlResponseDto
        {
            UploadUrl = uploadUrl,
            ObjectKey = objectKey,
            ExpiresInSeconds = UploadUrlExpirySeconds
        });
    }

    [HttpGet("view-url")]
    public async Task<IActionResult> CreateViewUrl([FromQuery] string objectKey)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUserService.KeycloakId))
        {
            return Unauthorized(new { message = "Utilisateur non authentifie." });
        }

        if (string.IsNullOrWhiteSpace(objectKey))
        {
            return BadRequest(new { message = "objectKey est obligatoire." });
        }

        if (!CanViewObject(objectKey, _currentUserService.KeycloakId))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Acces au fichier refuse." });
        }

        var viewUrl = await _storageService.GenerateViewUrlAsync(
            objectKey,
            TimeSpan.FromSeconds(ViewUrlExpirySeconds));

        return Ok(new ViewUrlResponseDto
        {
            ViewUrl = viewUrl,
            ExpiresInSeconds = ViewUrlExpirySeconds
        });
    }

    private bool CanViewObject(string objectKey, string userId)
    {
        if (_currentUserService.Roles.Contains(KeycloakConstants.RoleAdmin, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        // TODO: Refine business rules by context:
        // document driver -> admin + owner driver;
        // delivery proof -> client + driver of the delivery;
        // complaint evidence -> related client + admin.
        return objectKey.Contains($"/{userId}/", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildObjectKey(FileContext context, string userId, string extension)
    {
        var prefix = context switch
        {
            FileContext.PROFILE_PHOTO => "profile-photos",
            FileContext.DOCUMENT => "documents",
            FileContext.DELIVERY_PROOF => "delivery-proofs",
            FileContext.COMPLAINT_EVIDENCE => "complaint-evidence",
            _ => throw new ArgumentOutOfRangeException(nameof(context), context, "Invalid file context.")
        };

        return $"{prefix}/{userId}/{Guid.NewGuid():N}.{extension}";
    }

    private static string NormalizeFileType(string fileType)
    {
        return fileType.Trim().TrimStart('.').ToLowerInvariant();
    }
}
