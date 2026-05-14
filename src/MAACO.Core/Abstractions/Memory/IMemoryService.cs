using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Memory;

public interface IMemoryService
{
    Task SaveProjectSummaryAsync(Guid projectId, string summary, CancellationToken cancellationToken);
    Task SaveTaskSummaryAsync(Guid taskId, string summary, CancellationToken cancellationToken);
    Task SaveFileSummaryAsync(Guid projectId, string filePath, string summary, CancellationToken cancellationToken);
    Task SaveBuildFailureAsync(Guid taskId, string failureSummary, CancellationToken cancellationToken);
    Task SaveDecisionAsync(Guid projectId, string decision, CancellationToken cancellationToken);
    Task SaveAgentNoteAsync(Guid taskId, string note, CancellationToken cancellationToken);
    Task<IReadOnlyList<MemoryRecord>> ListByProjectIdAsync(Guid projectId, CancellationToken cancellationToken);
    Task<IReadOnlyList<MemoryRecord>> ListByTaskIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<IReadOnlyList<MemoryRecord>> SearchByProjectIdAsync(
        Guid projectId,
        string keyword,
        int topN,
        CancellationToken cancellationToken);
}
