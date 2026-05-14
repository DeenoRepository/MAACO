namespace MAACO.Core.Abstractions.Tools;

public interface IAgentTool
{
    string Name { get; }
    IReadOnlyCollection<ToolPermission> RequiredPermissions { get; }
    Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken);
}
