using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Complaints.DTOs;
using Wasel.Api.Modules.Complaints.Services;

namespace Wasel.Api.Modules.Complaints.Controllers;

[ApiController]
[Route("api/admin/complaints")]
[Authorize(Policy = "AdminOnly")]
public class AdminComplaintsController : ControllerBase
{
    private readonly IComplaintService _complaintService;

    public AdminComplaintsController(IComplaintService complaintService)
    {
        _complaintService = complaintService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAdminComplaints(
        [FromQuery] string? status,
        [FromQuery] string? type,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _complaintService.GetAdminComplaintsAsync(
                status,
                type,
                fromDate,
                toDate,
                page,
                pageSize);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAdminComplaintDetails(Guid id)
    {
        try
        {
            var result = await _complaintService.GetAdminComplaintDetailsAsync(id);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateComplaintStatus(
        Guid id,
        [FromBody] UpdateComplaintStatusRequestDto request)
    {
        try
        {
            var result = await _complaintService.UpdateComplaintStatusAsync(id, request);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/resolve")]
    public async Task<IActionResult> ResolveComplaint(
        Guid id,
        [FromBody] ResolveComplaintRequestDto request)
    {
        try
        {
            var result = await _complaintService.ResolveComplaintAsync(id, request);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}