using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Drivers.Services;

namespace Wasel.Api.Modules.Drivers.Controllers;

[ApiController]
[Route("api/admin/drivers")]
public class DriversAdminController : ControllerBase
{
    private readonly IDriverService _driverService;

    public DriversAdminController(IDriverService driverService)
    {
        _driverService = driverService;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingDrivers()
    {
        var drivers = await _driverService.GetPendingDriversAsync();
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
    
}