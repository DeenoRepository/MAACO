namespace MAACO.Agents.Abstractions;

public interface IAgentRegistry
{
    IReadOnlyCollection<string> ListAgentNames();
    IAgent? GetByName(string name);
}
