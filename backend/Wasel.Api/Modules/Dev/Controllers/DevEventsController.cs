using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Wasel.Api.Shared.EventBus;
using Wasel.Api.Shared.EventBus.IntegrationEvents;

namespace Wasel.Api.Modules.Dev.Controllers;

[ApiController]
[Route("api/dev/events")]
public class DevEventsController : ControllerBase
{
    private readonly IEventBus _eventBus;
    private readonly RabbitMqOptions _rabbitMqOptions;
    private readonly ILogger<DevEventsController> _logger;

    public DevEventsController(
        IEventBus eventBus,
        IOptions<RabbitMqOptions> rabbitMqOptions,
        ILogger<DevEventsController> logger)
    {
        _eventBus = eventBus;
        _rabbitMqOptions = rabbitMqOptions.Value;
        _logger = logger;
    }

    [HttpPost("test-notification")]
    public async Task<IActionResult> PublishTestNotification(
        [FromBody] TestNotificationRequest request,
        CancellationToken cancellationToken)
    {
        var notificationEvent = new NotificationRequestedEvent
        {
            RecipientUserId = request.RecipientUserId,
            Type = "TEST_NOTIFICATION",
            Title = request.Title,
            Message = request.Message,
            RelatedEntityType = "DEV_TEST",
            RelatedEntityId = null,
            Channels = new[] { "IN_APP", "PUSH" },
            CreatedAt = DateTime.UtcNow
        };

        await _eventBus.PublishAsync(
            notificationEvent,
            _rabbitMqOptions.NotificationRoutingKey,
            cancellationToken);

        _logger.LogInformation(
            "Test NotificationRequestedEvent published with routing key {RoutingKey}",
            _rabbitMqOptions.NotificationRoutingKey);

        return Ok(new
        {
            message = "NotificationRequestedEvent published successfully",
            routingKey = _rabbitMqOptions.NotificationRoutingKey,
            exchange = _rabbitMqOptions.ExchangeName,
            eventId = notificationEvent.EventId
        });
    }
}

public sealed class TestNotificationRequest
{
    public Guid RecipientUserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}