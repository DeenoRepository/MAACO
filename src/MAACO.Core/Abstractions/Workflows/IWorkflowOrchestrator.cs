using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Workflows;

public interface IWorkflowOrchestrator
{
    Task<Workflow> ExecuteAsync(
        WorkflowExecutionContext context,
        IReadOnlyList<string> stepNames,
        CancellationToken cancellationToken);
}
