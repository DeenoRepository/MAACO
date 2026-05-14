using MAACO.Core.Abstractions.Events;
using MAACO.Infrastructure.Events;
using MAACO.Infrastructure.Events.Handlers;
using Microsoft.Extensions.DependencyInjection;
using MAACO.Core.Domain.Events;

namespace MAACO.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMaacoInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddSingleton<IEventHandler<TaskCreatedEvent>, TaskCreatedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowStartedEvent>, WorkflowStartedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowStepStartedEvent>, WorkflowStepStartedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowStepCompletedEvent>, WorkflowStepCompletedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowStepFailedEvent>, WorkflowStepFailedEventLogHandler>();
        services.AddSingleton<IEventHandler<ApprovalRequestedEvent>, ApprovalRequestedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowCompletedEvent>, WorkflowCompletedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowFailedEvent>, WorkflowFailedEventLogHandler>();
        return services;
    }

    public static void UseMaacoInfrastructure(this IServiceProvider serviceProvider)
    {
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();

        foreach (var handler in serviceProvider.GetServices<IEventHandler<TaskCreatedEvent>>())
        {
            eventBus.Subscribe(handler);
        }

        foreach (var handler in serviceProvider.GetServices<IEventHandler<WorkflowStartedEvent>>())
        {
            eventBus.Subscribe(handler);
        }

        foreach (var handler in serviceProvider.GetServices<IEventHandler<WorkflowStepStartedEvent>>())
        {
            eventBus.Subscribe(handler);
        }

        foreach (var handler in serviceProvider.GetServices<IEventHandler<WorkflowStepCompletedEvent>>())
        {
            eventBus.Subscribe(handler);
        }

        foreach (var handler in serviceProvider.GetServices<IEventHandler<WorkflowStepFailedEvent>>())
        {
            eventBus.Subscribe(handler);
        }

        foreach (var handler in serviceProvider.GetServices<IEventHandler<ApprovalRequestedEvent>>())
        {
            eventBus.Subscribe(handler);
        }

        foreach (var handler in serviceProvider.GetServices<IEventHandler<WorkflowCompletedEvent>>())
        {
            eventBus.Subscribe(handler);
        }

        foreach (var handler in serviceProvider.GetServices<IEventHandler<WorkflowFailedEvent>>())
        {
            eventBus.Subscribe(handler);
        }
    }
}
