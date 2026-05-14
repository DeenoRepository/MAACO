using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Persistence.Data;

public static class DbSeed
{
    public static async Task InitializeAsync(MaacoDbContext dbContext, CancellationToken cancellationToken = default)
    {
        if (!await dbContext.AgentDefinitions.AnyAsync(cancellationToken))
        {
            var seedAgentsAndTools = new[]
            {
                new AgentDefinition { Name = "TaskPlannerAgent", Role = "Planner", Status = AgentStatus.Idle },
                new AgentDefinition { Name = "BackendDeveloperAgent", Role = "Developer", Status = AgentStatus.Idle },
                new AgentDefinition { Name = "DebuggingAgent", Role = "Debugger", Status = AgentStatus.Idle },
                new AgentDefinition { Name = "DocumentationAgent", Role = "Documenter", Status = AgentStatus.Idle },
                new AgentDefinition { Name = "FileSystemTool", Role = "Tool", Status = AgentStatus.Idle },
                new AgentDefinition { Name = "CodePatchTool", Role = "Tool", Status = AgentStatus.Idle },
                new AgentDefinition { Name = "BuildTool", Role = "Tool", Status = AgentStatus.Idle },
                new AgentDefinition { Name = "TestTool", Role = "Tool", Status = AgentStatus.Idle },
                new AgentDefinition { Name = "GitTool", Role = "Tool", Status = AgentStatus.Idle }
            };

            await dbContext.AgentDefinitions.AddRangeAsync(seedAgentsAndTools, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
