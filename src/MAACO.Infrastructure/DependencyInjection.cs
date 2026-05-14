using MAACO.Core.Abstractions.Events;
using MAACO.Core.Abstractions.Llm;
using MAACO.Infrastructure.Events;
using MAACO.Infrastructure.Events.Handlers;
using MAACO.Infrastructure.Llm;
using MAACO.Infrastructure.Workflows;
using MAACO.Infrastructure.Workflows.Steps;
using Microsoft.Extensions.DependencyInjection;
using MAACO.Core.Domain.Events;
using MAACO.Core.Abstractions.Workflows;

namespace MAACO.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMaacoInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, InMemoryEventBus>();
        services.AddSingleton<IEventHandler<TaskCreatedEvent>, TaskCreatedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowStartedEvent>, WorkflowStartedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowStartedEvent>, WorkflowStartedStatusHandler>();
        services.AddSingleton<IEventHandler<WorkflowStepStartedEvent>, WorkflowStepStartedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowStepCompletedEvent>, WorkflowStepCompletedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowStepFailedEvent>, WorkflowStepFailedEventLogHandler>();
        services.AddSingleton<IEventHandler<ApprovalRequestedEvent>, ApprovalRequestedEventLogHandler>();
        services.AddSingleton<IEventHandler<ApprovalRequestedEvent>, ApprovalRequestedStatusHandler>();
        services.AddSingleton<IEventHandler<WorkflowCompletedEvent>, WorkflowCompletedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowCompletedEvent>, WorkflowCompletedStatusHandler>();
        services.AddSingleton<IEventHandler<WorkflowFailedEvent>, WorkflowFailedEventLogHandler>();
        services.AddSingleton<IEventHandler<WorkflowFailedEvent>, WorkflowFailedStatusHandler>();
        services.AddSingleton(new LlmProviderOptions(
            Provider: "Fake",
            DefaultModel: "fake-default"));
        services.AddSingleton(new ModelRoutingPolicy(
            PlanningModel: "fake-planning",
            CodingModel: "fake-coding",
            DebuggingModel: "fake-debugging",
            SummaryModel: "fake-summary",
            FallbackModel: "fake-default"));
        services.AddSingleton<ILlmProvider, FakeLlmProvider>();
        services.AddSingleton<ILlmGateway, LlmGateway>();
        services.AddScoped<IWorkflowStepHandler, ProjectScanStepHandler>();
        services.AddScoped<IWorkflowStepHandler, GitBranchStepHandler>();
        services.AddScoped<IWorkflowStepHandler, PlanningStepHandler>();
        services.AddScoped<WorkflowStepExecutor>();
        services.AddScoped<IWorkflowOrchestrator, WorkflowOrchestrator>();
        return services;
    }

    public static void UseMaacoInfrastructure(this IServiceProvider serviceProvider)
    {
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();
        SubscribeAll<TaskCreatedEvent>(serviceProvider, eventBus);
        SubscribeAll<WorkflowStartedEvent>(serviceProvider, eventBus);
        SubscribeAll<WorkflowStepStartedEvent>(serviceProvider, eventBus);
        SubscribeAll<WorkflowStepCompletedEvent>(serviceProvider, eventBus);
        SubscribeAll<WorkflowStepFailedEvent>(serviceProvider, eventBus);
        SubscribeAll<LogReceivedEvent>(serviceProvider, eventBus);
        SubscribeAll<ToolExecutionStartedEvent>(serviceProvider, eventBus);
        SubscribeAll<ToolExecutionCompletedEvent>(serviceProvider, eventBus);
        SubscribeAll<ApprovalRequestedEvent>(serviceProvider, eventBus);
        SubscribeAll<WorkflowCompletedEvent>(serviceProvider, eventBus);
        SubscribeAll<WorkflowFailedEvent>(serviceProvider, eventBus);
    }

    private static void SubscribeAll<TEvent>(IServiceProvider serviceProvider, IEventBus eventBus)
        where TEvent : class
    {
        foreach (var handler in serviceProvider.GetServices<IEventHandler<TEvent>>())
        {
            eventBus.Subscribe(handler);
        }
    }
}
