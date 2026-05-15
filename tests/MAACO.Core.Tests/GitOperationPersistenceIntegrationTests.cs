using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.ValueObjects;
using MAACO.Persistence.Data;
using MAACO.Persistence.Repositories;
using MAACO.Tools.Tools;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MAACO.Core.Tests;

public sealed class GitOperationPersistenceIntegrationTests
{
    [Fact]
    public async Task GitTool_PersistsGitOperation_ToSqlite_WhenTaskIdProvided()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<MaacoDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new MaacoDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var project = new Project
        {
            Name = "MAACO Integration",
            RepositoryPath = new RepositoryPath("C:\\repo\\maaco-integration")
        };
        await dbContext.Projects.AddAsync(project);

        var task = new TaskItem
        {
            ProjectId = project.Id,
            Title = "Persist git operation",
            Status = MAACO.Core.Domain.Enums.TaskStatus.Created
        };
        await dbContext.TaskItems.AddAsync(task);
        await dbContext.SaveChangesAsync();

        var repoPath = CreateRealGitWorkspace();
        var gitOperationRepository = new GitOperationRepository(dbContext);
        var gitTool = new GitTool(gitOperationRepository);

        var request = new MAACO.Core.Abstractions.Tools.ToolRequest(
            ToolName: "GitTool",
            Input: $$"""{"operation":"push origin main","taskId":"{{task.Id}}"}""",
            WorkspacePath: repoPath,
            Permissions: [MAACO.Core.Abstractions.Tools.ToolPermission.ReadOnly],
            CorrelationId: "corr-gitop-sqlite");

        var result = await gitTool.ExecuteAsync(request, CancellationToken.None);

        Assert.False(result.Succeeded);
        var persisted = await dbContext.GitOperations.SingleAsync(x => x.TaskId == task.Id);
        Assert.Equal(GitOperationType.Status, persisted.Type);
        Assert.False(persisted.Succeeded);
        Assert.Contains("disabled in MVP", persisted.Details ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    private static string CreateRealGitWorkspace()
    {
        var path = Path.Combine(Path.GetTempPath(), "maaco-gitop-persist-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        Directory.CreateDirectory(Path.Combine(path, "src"));
        File.WriteAllText(Path.Combine(path, "src", "MAACO.Sample.txt"), "initial");

        RunGit(path, "init");
        RunGit(path, "config user.name \"maaco-tests\"");
        RunGit(path, "config user.email \"maaco-tests@local\"");
        RunGit(path, "add .");
        RunGit(path, "commit -m \"initial commit\"");
        return path;
    }

    private static void RunGit(string workingDirectory, string arguments)
    {
        var gitExecutable = ResolveGitExecutablePath();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = gitExecutable,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        _ = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"git {arguments} failed. {stderr}");
        }
    }

    private static string ResolveGitExecutablePath()
    {
        var candidates = new[]
        {
            @"C:\Program Files\Git\cmd\git.exe",
            @"C:\Program Files\Git\bin\git.exe",
            @"C:\Program Files (x86)\Git\cmd\git.exe",
            @"C:\Program Files (x86)\Git\bin\git.exe",
            "git"
        };

        foreach (var candidate in candidates)
        {
            if (string.Equals(candidate, "git", StringComparison.Ordinal) || File.Exists(candidate))
            {
                return candidate;
            }
        }

        return "git";
    }
}
