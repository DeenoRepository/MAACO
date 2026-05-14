using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using System.Collections.Concurrent;

namespace MAACO.Infrastructure.Workflows.Steps;

public sealed class TestStepHandler(ILogRepository logRepository) : IWorkflowStepHandler
{
    private static readonly ConcurrentDictionary<Guid, int> AttemptCounters = new();

    public string Name => "TestStep";

    public async Task ExecuteAsync(
        WorkflowExecutionContext context,
        WorkflowStep step,
        CancellationToken cancellationToken)
    {
        if (ShouldFail(context, "TestFailAttempts"))
        {
            throw new InvalidOperationException("Simulated test failure.");
        }

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = LogSeverity.Information,
                CorrelationId = context.CorrelationId,
                Message = $"Executed {Name} for workflow {context.WorkflowId:D}."
            },
            cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
    }

    private static bool ShouldFail(WorkflowExecutionContext context, string key)
    {
        if (context.Inputs is null ||
            !context.Inputs.TryGetValue(key, out var value) ||
            !int.TryParse(value, out var failAttempts) ||
            failAttempts <= 0)
        {
            return false;
        }

        var attempt = AttemptCounters.AddOrUpdate(context.WorkflowId, 1, (_, current) => current + 1);
        return attempt <= failAttempts;
    }
}
