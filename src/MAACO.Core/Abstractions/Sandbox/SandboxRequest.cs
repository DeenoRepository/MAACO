namespace MAACO.Core.Abstractions.Sandbox;

public sealed record SandboxRequest(
    string FileName,
    string Arguments,
    string WorkspacePath,
    SandboxOptions Options);
