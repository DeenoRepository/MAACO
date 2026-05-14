using MAACO.Agents.Abstractions;

namespace MAACO.Agents.Agents;

public abstract class StubAgentBase : IAgent
{
    public abstract string Name { get; }
    public abstract string Role { get; }
    public abstract IReadOnlyCollection<AgentCapability> Capabilities { get; }

    public virtual Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new AgentResult(
            Succeeded: true,
            Output: $"{Name} stub completed.",
            Error: null,
            Metadata: new Dictionary<string, string>
            {
                ["agent"] = Name,
                ["role"] = Role,
                ["capabilities"] = string.Join(",", Capabilities),
                ["workflowId"] = context.WorkflowId.ToString("D")
            }));
    }
}
