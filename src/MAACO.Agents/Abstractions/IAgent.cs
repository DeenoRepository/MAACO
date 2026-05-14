namespace MAACO.Agents.Abstractions;

public interface IAgent
{
    string Name { get; }
    string Role { get; }
    IReadOnlyCollection<AgentCapability> Capabilities { get; }

    Task<AgentResult> ExecuteAsync(
        AgentContext context,
        CancellationToken cancellationToken);
}
