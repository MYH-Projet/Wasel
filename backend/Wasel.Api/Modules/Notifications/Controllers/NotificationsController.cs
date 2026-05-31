using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wasel.Api.Modules.Notifications.DTOs;
using Wasel.Api.Modules.Notifications.Services;
using Wasel.Api.Shared.Exceptions;

namespace Wasel.Api.Modules.Notifications.Controllers;

[ApiController]
[Authorize(Policy = "ActiveUserOnly")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("api/notifications/my")]
    public async Task<ActionResult<NotificationsPageResponseDto>> GetMyNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _notificationService.GetMyNotificationsAsync(page, pageSize);
            return Ok(result);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPatch("api/notifications/{id:guid}/read")]
    public async Task<ActionResult<NotificationResponseDto>> MarkAsRead(Guid id)
    {
        try
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            return Ok(result);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }

    [HttpPatch("api/notifications/read-all")]
    public async Task<ActionResult<MarkAllReadResponseDto>> MarkAllAsRead()
    {
        try
        {
            var result = await _notificationService.MarkAllAsReadAsync();
            return Ok(result);
        }
        catch (ApiException ex)
        {
            return StatusCode(ex.StatusCode, new { message = ex.Message });
        }
    }
}
