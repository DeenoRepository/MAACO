namespace MAACO.Core.Abstractions.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class;

    void Subscribe<TEvent>(IEventHandler<TEvent> handler)
        where TEvent : class;
}
