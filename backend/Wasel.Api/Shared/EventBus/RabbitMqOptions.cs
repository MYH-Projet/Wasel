namespace Wasel.Api.Shared.EventBus;

public sealed class RabbitMqOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = "wasel.events";
    public string NotificationRoutingKey { get; set; } = "notification.requested";
}