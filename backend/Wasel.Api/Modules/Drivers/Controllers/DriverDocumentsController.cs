using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Documents.DTOs;
using Wasel.Api.Modules.Documents.Services;
using Wasel.Api.Shared.Security;

namespace Wasel.Api.Modules.Drivers.Controllers;

[ApiController]
[Route("api/drivers/dossier/documents")]
[Authorize(Policy = "ActiveUserOnly")]
public class DriverDocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ICurrentUserService _currentUserService;

    public DriverDocumentsController(IDocumentService documentService, ICurrentUserService currentUserService)
    {
        _documentService = documentService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    public async Task<ActionResult<DriverDocumentResponseDto>> AddOrReplaceDocument(
        [FromBody] AddDriverDocumentRequestDto request)
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (keycloakId == null)
            return Unauthorized();

        var result = await _documentService.AddOrReplaceCurrentDriverDocumentAsync(keycloakId, request);
        
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DriverDocumentResponseDto>>> GetDocuments()
    {
        var keycloakId = _currentUserService.KeycloakId;
        if (keycloakId == null)
            return Unauthorized();

        var result = await _documentService.GetCurrentDriverDocumentsAsync(keycloakId);
        
        return Ok(result);
    }
}
