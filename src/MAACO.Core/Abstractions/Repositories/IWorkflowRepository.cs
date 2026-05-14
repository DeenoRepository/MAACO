using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface IWorkflowRepository
{
    Task<Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Workflow?> GetLatestByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<IReadOnlyList<WorkflowStep>> ListStepsAsync(Guid workflowId, CancellationToken cancellationToken);
    Task AddWorkflowAsync(Workflow workflow, CancellationToken cancellationToken);
    Task AddStepAsync(WorkflowStep step, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
