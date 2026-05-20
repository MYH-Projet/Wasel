using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Drivers.DTOs;
using Wasel.Api.Modules.Drivers.Services;

namespace Wasel.Api.Modules.Drivers.Controllers;

[ApiController]
[Route("api/admin/driver-dossiers")]
public class DriverDossiersAdminController : ControllerBase
{
    private readonly IDriverDossierService _driverDossierService;

    public DriverDossiersAdminController(IDriverDossierService driverDossierService)
    {
        _driverDossierService = driverDossierService;
    }

    [HttpPatch("{dossierId:guid}/status")]
    public async Task<IActionResult> UpdateDossierStatus(
        Guid dossierId,
        [FromBody] UpdateDriverDossierStatusRequestDto request)
    {
        try
        {
            var updated = await _driverDossierService.UpdateDossierStatusAsync(
                dossierId,
                request
            );

            if (!updated)
            {
                return NotFound(new { message = "Driver dossier not found" });
            }

            return Ok(new { message = "Driver dossier status updated successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}