using MAACO.Api.Services;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.ValueObjects;
using MAACO.Persistence.Data;
using MAACO.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace MAACO.Core.Tests;

public sealed class ProjectScannerAcceptanceTests
{
    [Fact]
    public async Task ScanFlow_SavesSnapshotAndDetectsStackAndCommands()
    {
        var repoRoot = Path.Combine(Path.GetTempPath(), "maaco-scan-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(repoRoot);
        Directory.CreateDirectory(Path.Combine(repoRoot, ".git"));
        await File.WriteAllTextAsync(Path.Combine(repoRoot, "MAACO.sln"), "Microsoft Visual Studio Solution File");
        await File.WriteAllTextAsync(
            Path.Combine(repoRoot, "MAACO.Core.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");

        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<MaacoDbContext>().UseSqlite(connection).Options;
        await using var dbContext = new MaacoDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var project = new Project
        {
            Name = "LocalRepo",
            RepositoryPath = new RepositoryPath(repoRoot)
        };
        await dbContext.Projects.AddAsync(project);
        await dbContext.SaveChangesAsync();

        var scanner = new ProjectScanner();
        var scanResult = await scanner.ScanAsync(repoRoot, CancellationToken.None);

        var stackDetector = new ProjectStackDetector();
        var stack = await stackDetector.DetectAsync(repoRoot, scanResult.Files, CancellationToken.None);

        var settingsService = new InMemorySettingsService();
        var commandDetector = new ProjectBuildTestCommandDetector(settingsService);
        var commands = await commandDetector.DetectAsync(stack, stack.PackageManifests, CancellationToken.None);

        var snapshotRepository = new ProjectContextSnapshotRepository(dbContext);
        var snapshot = new ProjectContextSnapshot
        {
            ProjectId = project.Id,
            BranchName = "unknown",
            CommitHash = "unknown",
            Stack = new DetectedProjectStack(stack.PrimaryStack, "detected"),
            MetadataJson = "{\"summary\":\"ok\"}"
        };
        await snapshotRepository.AddAsync(snapshot, CancellationToken.None);
        await snapshotRepository.SaveChangesAsync(CancellationToken.None);

        var savedSnapshot = await dbContext.ProjectContextSnapshots.SingleAsync();
        Assert.Equal(project.Id, savedSnapshot.ProjectId);
        Assert.Equal(".NET", stack.PrimaryStack);
        Assert.Equal("dotnet build", commands.BuildCommand);
        Assert.Equal("dotnet test", commands.TestCommand);
    }
}
