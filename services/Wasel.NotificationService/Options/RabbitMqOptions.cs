namespace Wasel.NotificationService.Options;

public class RabbitMqOptions
{
    public const string SectionName = "RabbitMQ";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "wasel.events";
    public string QueueName { get; set; } = "notification.requested";
    public string RoutingKey { get; set; } = "notification.requested";
}
