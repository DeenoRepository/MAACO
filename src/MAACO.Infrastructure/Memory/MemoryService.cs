using MAACO.Core.Abstractions.Memory;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;

namespace MAACO.Infrastructure.Memory;

public sealed class MemoryService(
    IMemoryRepository memoryRepository,
    ITaskRepository taskRepository) : IMemoryService
{
    public Task SaveProjectSummaryAsync(Guid projectId, string summary, CancellationToken cancellationToken) =>
        SaveAsync(projectId, MemoryRecordType.Summary, "ProjectSummary", summary, cancellationToken);

    public async Task SaveTaskSummaryAsync(Guid taskId, string summary, CancellationToken cancellationToken)
    {
        var taskItem = await GetTaskOrThrowAsync(taskId, cancellationToken);
        await SaveAsync(taskItem.ProjectId, MemoryRecordType.Summary, $"TaskSummary:{taskId:D}", summary, cancellationToken);
    }

    public Task SaveFileSummaryAsync(Guid projectId, string filePath, string summary, CancellationToken cancellationToken) =>
        SaveAsync(projectId, MemoryRecordType.Summary, $"FileSummary:{filePath}", summary, cancellationToken);

    public async Task SaveBuildFailureAsync(Guid taskId, string failureSummary, CancellationToken cancellationToken)
    {
        var taskItem = await GetTaskOrThrowAsync(taskId, cancellationToken);
        await SaveAsync(taskItem.ProjectId, MemoryRecordType.Observation, $"BuildFailure:{taskId:D}", failureSummary, cancellationToken);
    }

    public Task SaveDecisionAsync(Guid projectId, string decision, CancellationToken cancellationToken) =>
        SaveAsync(projectId, MemoryRecordType.Decision, "Decision", decision, cancellationToken);

    public async Task SaveAgentNoteAsync(Guid taskId, string note, CancellationToken cancellationToken)
    {
        var taskItem = await GetTaskOrThrowAsync(taskId, cancellationToken);
        await SaveAsync(taskItem.ProjectId, MemoryRecordType.Observation, $"AgentNote:{taskId:D}", note, cancellationToken);
    }

    public Task<IReadOnlyList<MemoryRecord>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken) =>
        memoryRepository.ListByProjectIdAsync(projectId, cancellationToken);

    public async Task<IReadOnlyList<MemoryRecord>> ListByTaskIdAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var taskItem = await GetTaskOrThrowAsync(taskId, cancellationToken);
        var projectRecords = await memoryRepository.ListByProjectIdAsync(taskItem.ProjectId, cancellationToken);
        var taskToken = taskId.ToString("D");

        return projectRecords
            .Where(x => x.Key.Contains(taskToken, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<IReadOnlyList<MemoryRecord>> SearchByProjectIdAsync(
        Guid projectId,
        string keyword,
        int topN,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyword);
        if (topN <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(topN), "topN must be greater than zero.");
        }

        var normalizedKeyword = keyword.Trim();
        var projectRecords = await memoryRepository.ListByProjectIdAsync(projectId, cancellationToken);

        return projectRecords
            .Where(x =>
                x.Key.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase) ||
                x.Value.Contains(normalizedKeyword, StringComparison.OrdinalIgnoreCase))
            .Take(topN)
            .ToList();
    }

    private async Task SaveAsync(
        Guid projectId,
        MemoryRecordType type,
        string key,
        string value,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        await memoryRepository.AddAsync(
            new MemoryRecord
            {
                ProjectId = projectId,
                Type = type,
                Key = key,
                Value = value
            },
            cancellationToken);
        await memoryRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<TaskItem> GetTaskOrThrowAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var taskItem = await taskRepository.GetByIdAsync(taskId, cancellationToken);
        return taskItem ?? throw new InvalidOperationException($"Task {taskId:D} was not found.");
    }
}
