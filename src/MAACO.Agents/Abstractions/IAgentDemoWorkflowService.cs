namespace MAACO.Agents.Abstractions;

public interface IAgentDemoWorkflowService
{
    Task<AgentResult> RunAsync(AgentContext context, CancellationToken cancellationToken);
}
