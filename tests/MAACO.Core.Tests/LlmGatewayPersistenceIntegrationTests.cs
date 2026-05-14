using MAACO.Core.Abstractions.Llm;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.ValueObjects;
using MAACO.Infrastructure;
using MAACO.Persistence;
using MAACO.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MAACO.Core.Tests;

public sealed class LlmGatewayPersistenceIntegrationTests
{
    [Fact]
    public async Task LlmGateway_ThroughDi_PersistsLlmCallLog_UsingFakeProviderWithoutApiKey()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), "maaco-llm-gateway-integration", Guid.NewGuid().ToString("N"), "maaco.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        var connectionString = $"Data Source={dbPath}";

        var services = new ServiceCollection();
        services.AddMaacoPersistence(connectionString);
        services.AddMaacoInfrastructure();

        await using var provider = services.BuildServiceProvider();
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            await db.Database.EnsureCreatedAsync();

            var project = new Project
            {
                Name = "LLM Integration",
                RepositoryPath = new RepositoryPath("C:\\repo\\maaco-llm-integration")
            };
            await db.Projects.AddAsync(project);

            var workflow = new Workflow
            {
                TaskId = Guid.NewGuid()
            };
            await db.Workflows.AddAsync(workflow);
            await db.SaveChangesAsync();

            var gateway = scope.ServiceProvider.GetRequiredService<ILlmGateway>();
            var response = await gateway.GenerateAsync(
                new LlmRequest(
                    Messages: [new LlmMessage(LlmMessageRole.User, "token=supersecret plan the work")],
                    TaskType: LlmTaskType.Planning,
                    WorkflowId: workflow.Id,
                    CorrelationId: "corr-llm-di"),
                CancellationToken.None);

            Assert.True(response.Succeeded);
        }

        await using (var verifyScope = provider.CreateAsyncScope())
        {
            var db = verifyScope.ServiceProvider.GetRequiredService<MaacoDbContext>();
            var logs = await db.LlmCallLogs.ToListAsync();

            Assert.Single(logs);
            Assert.Equal("Fake", logs[0].Provider);
            Assert.True(logs[0].Usage.TotalTokens > 0);
            Assert.Contains("***REDACTED***", logs[0].MetadataJson ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("supersecret", logs[0].MetadataJson ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        }
    }
}
