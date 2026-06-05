namespace Wasel.Api.Shared.EventBus;

public interface IEventBus
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        string routingKey,
        CancellationToken cancellationToken = default);
}