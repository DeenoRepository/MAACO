using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Persistence.Data;

namespace MAACO.Persistence.Repositories;

public sealed class LlmCallLogRepository(MaacoDbContext dbContext) : ILlmCallLogRepository
{
    public Task AddAsync(LlmCallLog llmCallLog, CancellationToken cancellationToken) =>
        dbContext.LlmCallLogs.AddAsync(llmCallLog, cancellationToken).AsTask();

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
