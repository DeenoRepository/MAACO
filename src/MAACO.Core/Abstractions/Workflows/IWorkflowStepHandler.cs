using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Workflows;

public interface IWorkflowStepHandler
{
    string Name { get; }

    Task ExecuteAsync(
        WorkflowExecutionContext context,
        WorkflowStep step,
        CancellationToken cancellationToken);
}
