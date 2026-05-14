using MAACO.Core.Domain.Entities;

namespace MAACO.Core.Abstractions.Repositories;

public interface ILlmCallLogRepository
{
    Task AddAsync(LlmCallLog llmCallLog, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
