using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Repositories;

public sealed class WorkflowRepository(MaacoDbContext dbContext) : IWorkflowRepository
{
    public Task<Workflow?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Workflows.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<Workflow?> GetLatestByTaskIdAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var workflows = await dbContext.Workflows
            .Where(x => x.TaskId == taskId)
            .ToListAsync(cancellationToken);

        return workflows
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefault();
    }

    public async Task<IReadOnlyList<WorkflowStep>> ListStepsAsync(Guid workflowId, CancellationToken cancellationToken) =>
        await dbContext.WorkflowSteps.Where(x => x.WorkflowId == workflowId).OrderBy(x => x.Order).ToListAsync(cancellationToken);

    public Task AddWorkflowAsync(Workflow workflow, CancellationToken cancellationToken) =>
        dbContext.Workflows.AddAsync(workflow, cancellationToken).AsTask();

    public Task AddStepAsync(WorkflowStep step, CancellationToken cancellationToken) =>
        dbContext.WorkflowSteps.AddAsync(step, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
