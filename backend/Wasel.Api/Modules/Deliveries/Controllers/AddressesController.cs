using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Wasel.Api.Modules.Deliveries.DTOs;
using Wasel.Api.Modules.Deliveries.Services;
using Wasel.Api.Modules.Users.Repositories;

namespace Wasel.Api.Modules.Deliveries.Controllers;

[ApiController]
[Route("api/addresses")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;
    private readonly IUserRepository _userRepository;

    public AddressesController(
        IAddressService addressService,
        IUserRepository userRepository)
    {
        _addressService = addressService;
        _userRepository = userRepository;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequestDto request)
    {
        var user = await GetCurrentUserAsync();

        if (user is null)
            return Unauthorized(new { message = "Utilisateur introuvable." });

        var result = await _addressService.CreateAsync(user.Id, request);

        return CreatedAtAction(nameof(GetMyAddresses), result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyAddresses()
    {
        var user = await GetCurrentUserAsync();

        if (user is null)
            return Unauthorized(new { message = "Utilisateur introuvable." });

        var result = await _addressService.GetMyAddressesAsync(user.Id);

        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid id)
    {
        var user = await GetCurrentUserAsync();

        if (user is null)
            return Unauthorized(new { message = "Utilisateur introuvable." });

        try
        {
            await _addressService.DeleteAsync(user.Id, id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    private async Task<Wasel.Api.Modules.Users.Entities.User?> GetCurrentUserAsync()
    {
        var keycloakId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrWhiteSpace(keycloakId))
            return null;

        return await _userRepository.GetByKeycloakIdAsync(keycloakId);
    }
}