using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Wasel.NotificationService.DTOs;
using Wasel.NotificationService.Options;
using Wasel.NotificationService.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Wasel.NotificationService.Consumers;

public class NotificationRequestedConsumer : BackgroundService
{
    private readonly ILogger<NotificationRequestedConsumer> _logger;
    private readonly RabbitMqOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IChannel? _channel;

    public NotificationRequestedConsumer(
        ILogger<NotificationRequestedConsumer> logger,
        IOptions<RabbitMqOptions> options,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _options = options.Value;
        _serviceProvider = serviceProvider;
    }

    private async Task<bool> ConnectWithRetryAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password
        };

        int maxAttempts = 10;
        int[] backoffSeconds = { 1, 2, 5, 10, 10, 10, 10, 10, 10, 10 };

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                _logger.LogInformation("Attempt {Attempt}/{MaxAttempts} to connect to RabbitMQ at {Host}:{Port}", attempt, maxAttempts, _options.Host, _options.Port);
                
                _connection = await factory.CreateConnectionAsync(cancellationToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

                await _channel.ExchangeDeclareAsync(exchange: _options.ExchangeName, type: ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
                await _channel.QueueDeclareAsync(queue: _options.QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
                await _channel.QueueBindAsync(queue: _options.QueueName, exchange: _options.ExchangeName, routingKey: _options.RoutingKey, cancellationToken: cancellationToken);

                _logger.LogInformation("Successfully connected to RabbitMQ. Exchange: {Exchange}, Queue: {Queue}", _options.ExchangeName, _options.QueueName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ connection attempt {Attempt} failed. Retrying in {Delay}s...", attempt, backoffSeconds[attempt - 1]);
                
                if (attempt == maxAttempts)
                {
                    _logger.LogError("Max connection attempts ({MaxAttempts}) reached. Could not connect to RabbitMQ.", maxAttempts);
                    return false;
                }

                try 
                {
                    await Task.Delay(TimeSpan.FromSeconds(backoffSeconds[attempt - 1]), cancellationToken);
                }
                catch (TaskCanceledException) 
                { 
                    return false; 
                }
            }
        }

        return false;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connected = await ConnectWithRetryAsync(stoppingToken);
        if (!connected || _channel == null)
        {
            _logger.LogCritical("Failed to connect to RabbitMQ after retries. Consumer will not start.");
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            NotificationRequestedEvent? requestedEvent = null;

            try
            {
                requestedEvent = JsonSerializer.Deserialize<NotificationRequestedEvent>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (requestedEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize message: {Json}", json);
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                    return;
                }

                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<INotificationProcessor>();
                
                await processor.ProcessAsync(requestedEvent, stoppingToken);

                // ACK only after successful processing
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON message received. Acking to avoid infinite loop. Message: {Json}", json);
                await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification event {EventId}. Nacking to requeue.", requestedEvent?.EventId);
                // Nack and requeue so we don't lose it if it's a transient DB/Firebase issue.
                await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(queue: _options.QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
        _logger.LogInformation("RabbitMQ Consumer started, listening to queue {QueueName}", _options.QueueName);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync(cancellationToken);
        if (_connection != null) await _connection.CloseAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }
}
