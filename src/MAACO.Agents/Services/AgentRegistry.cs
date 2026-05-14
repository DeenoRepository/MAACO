using MAACO.Agents.Abstractions;

namespace MAACO.Agents.Services;

public sealed class AgentRegistry(IEnumerable<IAgent> agents) : IAgentRegistry
{
    private readonly Dictionary<string, IAgent> agentMap = agents.ToDictionary(
        x => x.Name,
        x => x,
        StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> ListAgentNames() => agentMap.Keys.ToArray();

    public IAgent? GetByName(string name) =>
        agentMap.TryGetValue(name, out var agent) ? agent : null;
}
