using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Drivers.Services;
using Microsoft.AspNetCore.Authorization;
namespace Wasel.Api.Modules.Drivers.Controllers;

[ApiController]
[Route("api/admin/drivers")]
[Authorize(Policy = "AdminOnly")]
public class DriversAdminController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriversAdminController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingDrivers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var drivers = await _driverService.GetPendingDriversAsync(
            page,
            pageSize,
            search);

        return Ok(drivers);
    }

    [HttpGet("{driverId:guid}/dossier")]
    public async Task<IActionResult> GetDriverDossier(Guid driverId)
    {
        var dossier = await _driverService.GetDriverDossierAsync(driverId);

        if (dossier is null)
        {
            return NotFound(new { message = "Driver not found" });
        }

        return Ok(dossier);
    }

    [HttpGet]
    public async Task<IActionResult> GetAdminDrivers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? driverStatus = null,
        [FromQuery] string? dossierStatus = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var result = await _driverService.GetAdminDriversAsync(
                page,
                pageSize,
                search,
                driverStatus,
                dossierStatus,
                startDate,
                endDate);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
}