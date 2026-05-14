namespace MAACO.Agents.Abstractions;

public interface IAgentExecutionService
{
    Task<AgentResult> ExecuteAsync(
        string agentName,
        AgentContext context,
        CancellationToken cancellationToken);
}
