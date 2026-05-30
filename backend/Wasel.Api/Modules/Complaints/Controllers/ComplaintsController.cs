using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wasel.Api.Modules.Complaints.DTOs;
using Wasel.Api.Modules.Complaints.Services;

namespace Wasel.Api.Modules.Complaints.Controllers;

[ApiController]
[Route("api/complaints")]
[Authorize]
public class ComplaintsController : ControllerBase
{
    private readonly IComplaintService _complaintService;

    public ComplaintsController(IComplaintService complaintService)
    {
        _complaintService = complaintService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateComplaint([FromBody] CreateComplaintRequestDto request)
    {
        var keycloakId = User.FindFirstValue("sub")
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.Claims.FirstOrDefault(c => c.Type.EndsWith("/nameidentifier"))?.Value;

        if (keycloakId is null)
            return Unauthorized(new { message = "Utilisateur non authentifié." });

        try
        {
            var result = await _complaintService.CreateComplaintAsync(request, keycloakId);

            return Created("", result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/evidence")]
    public async Task<IActionResult> AddEvidence(Guid id, [FromBody] AddComplaintEvidenceRequestDto request)
    {
        var keycloakId = User.FindFirstValue("sub")
                    ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.Claims.FirstOrDefault(c => c.Type.EndsWith("/nameidentifier"))?.Value;

        if (keycloakId is null)
            return Unauthorized(new { message = "Utilisateur non authentifié." });

        try
        {
            var result = await _complaintService.AddEvidenceAsync(id, request, keycloakId);

            return Created("", result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("my")]
    public async Task<IActionResult> GetMyComplaints(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var keycloakId = User.FindFirstValue("sub")
                    ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.Claims.FirstOrDefault(c => c.Type.EndsWith("/nameidentifier"))?.Value;

        if (keycloakId is null)
            return Unauthorized(new { message = "Utilisateur non authentifié." });

        try
        {
            var result = await _complaintService.GetMyComplaintsAsync(
                keycloakId,
                status,
                page,
                pageSize);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetComplaintDetails(Guid id)
    {
        var keycloakId = User.FindFirstValue("sub")
                    ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.Claims.FirstOrDefault(c => c.Type.EndsWith("/nameidentifier"))?.Value;

        if (keycloakId is null)
            return Unauthorized(new { message = "Utilisateur non authentifié." });

        try
        {
            var result = await _complaintService.GetComplaintDetailsAsync(id, keycloakId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}