using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Users.Services;
using Wasel.Api.Modules.Users.DTOs;

using Microsoft.AspNetCore.Authorization;

namespace Wasel.Api.Modules.Users.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "AdminOnly")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? role = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var users = await _userService.GetAdminUsersAsync(
                page,
                pageSize,
                search,
                role,
                status,
                startDate,
                endDate);

            return Ok(users);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);

        if (user is null)
        {
            return NotFound(new
            {
                message = "User not found"
            });
        }

        return Ok(user);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeUserStatus(Guid id, ChangeUserStatusRequestDto request)
    {
        var user = await _userService.ChangeUserStatusAsync(id, request);

        if (user is null)
        {
            return NotFound(new
            {
                message = "User not found"
            });
        }

        return Ok(user);
    }
}