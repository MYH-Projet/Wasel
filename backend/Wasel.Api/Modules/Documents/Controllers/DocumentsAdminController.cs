using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Documents.DTOs;
using Wasel.Api.Modules.Documents.Services;
using Microsoft.AspNetCore.Authorization;
namespace Wasel.Api.Modules.Documents.Controllers;

[ApiController]
[Route("api/admin/documents")]
[Authorize(Policy = "AdminOnly")]
public class DocumentsAdminController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsAdminController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpPatch("{documentId:guid}/{status}")]
    public async Task<IActionResult> UpdateDocumentStatus(
        Guid documentId,
        string status,
        [FromBody] RejectDocumentRequestDto? request)
    {
        try
        {
            var updated = await _documentService.UpdateDocumentStatusAsync(
                documentId,
                status,
                request
            );

            if (!updated)
            {
                return NotFound(new { message = "Document not found" });
            }

            return Ok(new { message = "Document status updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}