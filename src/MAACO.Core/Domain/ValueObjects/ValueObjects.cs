namespace MAACO.Core.Domain.ValueObjects;

public sealed record RepositoryPath
{
    public RepositoryPath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Repository path cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}

public sealed record CommandSpec(
    string FileName,
    string Arguments,
    string WorkingDirectory,
    TimeSpan Timeout);

public sealed record LlmUsage(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens,
    string? Model = null);

public sealed record PatchSummary(
    int FilesChanged,
    int AddedLines,
    int RemovedLines,
    string? Description = null);

public sealed record ExecutionResult(
    int ExitCode,
    string StdOut,
    string StdErr,
    TimeSpan Duration,
    bool TimedOut = false);

public sealed record DetectedProjectStack(
    string PrimaryLanguage,
    string Framework,
    string? Database = null,
    string? Runtime = null);

public sealed record FileChangeSummary(
    string Path,
    int AddedLines,
    int RemovedLines,
    bool IsBinary);
