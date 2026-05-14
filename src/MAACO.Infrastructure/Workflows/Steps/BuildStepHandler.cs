using MAACO.Core.Abstractions.Events;
using MAACO.Core.Abstractions.Repositories;
using MAACO.Core.Abstractions.Workflows;
using MAACO.Core.Domain.Entities;
using MAACO.Core.Domain.Enums;
using MAACO.Core.Domain.Events;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MAACO.Infrastructure.Workflows.Steps;

public sealed class BuildStepHandler(
    ILogRepository logRepository,
    IBuildRunRepository buildRunRepository,
    IArtifactRepository artifactRepository,
    IEventBus eventBus) : IWorkflowStepHandler
{
    private static readonly ConcurrentDictionary<Guid, int> AttemptCounters = new();
    private static readonly Regex CompilerErrorRegex = new(@"\berror\b\s+([A-Za-z]+\d+)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex FailedAssertionRegex = new(@"\b(Assert\.\w+|Expected:|Actual:|Assertion)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public string Name => "BuildStep";

    public async Task ExecuteAsync(
        WorkflowExecutionContext context,
        WorkflowStep step,
        CancellationToken cancellationToken)
    {
        var hasDetectedCommand = TryResolveBuildCommand(context, out var command, out var arguments, out var workingDirectory);
        if (hasDetectedCommand)
        {
            await ExecuteDetectedBuildCommandAsync(
                context,
                command,
                arguments,
                workingDirectory,
                cancellationToken);
            return;
        }

        if (ShouldFail(context, "BuildFailAttempts"))
        {
            throw new InvalidOperationException("Simulated build failure.");
        }

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = LogSeverity.Information,
                CorrelationId = context.CorrelationId,
                Message = $"Executed {Name} for workflow {context.WorkflowId:D}."
            },
            cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task ExecuteDetectedBuildCommandAsync(
        WorkflowExecutionContext context,
        string command,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        await eventBus.PublishAsync(
            new ToolExecutionStartedEvent(
                context.WorkflowId,
                "BuildTool",
                DateTimeOffset.UtcNow,
                context.CorrelationId),
            cancellationToken);

        var startedAt = DateTimeOffset.UtcNow;
        var buildRun = new BuildRun
        {
            WorkflowId = context.WorkflowId,
            Status = BuildRunStatus.Started
        };
        await buildRunRepository.AddAsync(buildRun, cancellationToken);
        await buildRunRepository.SaveChangesAsync(cancellationToken);

        var (exitCode, stdOut, stdErr) = await RunProcessAsync(
            command,
            arguments,
            workingDirectory,
            cancellationToken);

        buildRun.Duration = DateTimeOffset.UtcNow - startedAt;
        buildRun.Status = exitCode == 0 ? BuildRunStatus.Succeeded : BuildRunStatus.Failed;
        await buildRunRepository.SaveChangesAsync(cancellationToken);

        await SaveBuildArtifactsAsync(context, stdOut, stdErr, exitCode, cancellationToken);
        await PersistDiagnosticsSummaryAsync(context, stdOut, stdErr, cancellationToken);

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = exitCode == 0 ? LogSeverity.Information : LogSeverity.Error,
                CorrelationId = context.CorrelationId,
                Message = $"Executed {Name} detected command '{command} {arguments}'. ExitCode={exitCode}."
            },
            cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);

        await eventBus.PublishAsync(
            new ToolExecutionCompletedEvent(
                context.WorkflowId,
                "BuildTool",
                exitCode == 0,
                DateTimeOffset.UtcNow,
                context.CorrelationId),
            cancellationToken);

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Build command failed with exit code {exitCode}.");
        }
    }

    private async Task SaveBuildArtifactsAsync(
        WorkflowExecutionContext context,
        string stdOut,
        string stdErr,
        int exitCode,
        CancellationToken cancellationToken)
    {
        var outPayload = Truncate(stdOut, 40000);
        var errPayload = Truncate(stdErr, 40000);

        await artifactRepository.AddAsync(
            new Artifact
            {
                TaskId = context.TaskId,
                Type = ArtifactType.BuildLog,
                Path = $"build://workflow/{context.WorkflowId:D}/stdout",
                Hash = $"exit={exitCode};len={outPayload.Length}"
            },
            cancellationToken);

        await artifactRepository.AddAsync(
            new Artifact
            {
                TaskId = context.TaskId,
                Type = ArtifactType.BuildLog,
                Path = $"build://workflow/{context.WorkflowId:D}/stderr",
                Hash = $"exit={exitCode};len={errPayload.Length}"
            },
            cancellationToken);

        await artifactRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task PersistDiagnosticsSummaryAsync(
        WorkflowExecutionContext context,
        string stdOut,
        string stdErr,
        CancellationToken cancellationToken)
    {
        var lines = (stdOut + Environment.NewLine + stdErr)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var compilerErrors = lines.Where(line => CompilerErrorRegex.IsMatch(line)).Distinct(StringComparer.Ordinal).Take(20).ToList();
        var stackTraces = lines.Where(line => line.StartsWith("at ", StringComparison.Ordinal) || line.Contains("--- End of stack trace", StringComparison.OrdinalIgnoreCase)).Distinct(StringComparer.Ordinal).Take(20).ToList();
        var failedAssertions = lines.Where(line => FailedAssertionRegex.IsMatch(line)).Distinct(StringComparer.Ordinal).Take(20).ToList();

        await logRepository.AddAsync(
            new LogEvent
            {
                WorkflowId = context.WorkflowId,
                TaskId = context.TaskId,
                Severity = LogSeverity.Information,
                CorrelationId = context.CorrelationId,
                Message = $"Diagnostics summary: CompilerErrors={compilerErrors.Count}; StackTraces={stackTraces.Count}; FailedAssertions={failedAssertions.Count}."
            },
            cancellationToken);
        await logRepository.SaveChangesAsync(cancellationToken);
    }

    private static bool ShouldFail(WorkflowExecutionContext context, string key)
    {
        if (context.Inputs is null ||
            !context.Inputs.TryGetValue(key, out var value) ||
            !int.TryParse(value, out var failAttempts) ||
            failAttempts <= 0)
        {
            return false;
        }

        var attempt = AttemptCounters.AddOrUpdate(context.WorkflowId, 1, (_, current) => current + 1);
        return attempt <= failAttempts;
    }

    private static bool TryResolveBuildCommand(
        WorkflowExecutionContext context,
        out string command,
        out string arguments,
        out string workingDirectory)
    {
        command = string.Empty;
        arguments = string.Empty;
        workingDirectory = Environment.CurrentDirectory;

        if (context.Inputs is null ||
            !context.Inputs.TryGetValue("BuildCommand", out var buildCommand) ||
            string.IsNullOrWhiteSpace(buildCommand))
        {
            return false;
        }

        var parsed = ParseCommandLine(buildCommand.Trim());
        if (parsed.Count == 0)
        {
            return false;
        }

        command = parsed[0];
        arguments = string.Join(' ', parsed.Skip(1));
        if (context.Inputs.TryGetValue("WorkspacePath", out var workspacePath) &&
            !string.IsNullOrWhiteSpace(workspacePath))
        {
            workingDirectory = workspacePath;
        }

        return true;
    }

    private static async Task<(int ExitCode, string StdOut, string StdErr)> RunProcessAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return (process.ExitCode, await stdOutTask, await stdErrTask);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];

    private static List<string> ParseCommandLine(string commandLine)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var c in commandLine)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            result.Add(current.ToString());
        }

        return result;
    }
}
