namespace MAACO.Core.Abstractions.Tools;

public interface IToolRegistry
{
    IReadOnlyCollection<string> ListToolNames();
    Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken);
}
