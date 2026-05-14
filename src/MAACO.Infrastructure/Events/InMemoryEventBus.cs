using MAACO.Core.Abstractions.Events;
using System.Collections.Concurrent;

namespace MAACO.Infrastructure.Events;

public sealed class InMemoryEventBus : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<object>> handlers = new();

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : class
    {
        if (!handlers.TryGetValue(typeof(TEvent), out var registeredHandlers))
        {
            return Task.CompletedTask;
        }

        IEventHandler<TEvent>[] snapshot;
        lock (registeredHandlers)
        {
            snapshot = registeredHandlers.Cast<IEventHandler<TEvent>>().ToArray();
        }

        return PublishToHandlersAsync(snapshot, @event, cancellationToken);
    }

    public void Subscribe<TEvent>(IEventHandler<TEvent> handler)
        where TEvent : class
    {
        var list = handlers.GetOrAdd(typeof(TEvent), _ => []);
        lock (list)
        {
            list.Add(handler);
        }
    }

    private static async Task PublishToHandlersAsync<TEvent>(
        IEnumerable<IEventHandler<TEvent>> eventHandlers,
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : class
    {
        foreach (var eventHandler in eventHandlers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await eventHandler.HandleAsync(@event, cancellationToken);
        }
    }
}
