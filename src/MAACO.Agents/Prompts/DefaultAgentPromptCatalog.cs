namespace MAACO.Agents.Prompts;

public sealed class DefaultAgentPromptCatalog : IAgentPromptCatalog
{
    private static readonly IReadOnlyDictionary<string, string> SystemPrompts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["TaskPlannerAgent"] =
            "You are the MAACO Planner. Produce a deterministic, minimal step plan. Respect workspace boundaries, approval gates, and milestone order.",
        ["BackendDeveloperAgent"] =
            "You are the MAACO Developer. Generate small, safe, testable implementation changes. Prefer existing architecture and avoid speculative refactors.",
        ["TestWriterAgent"] =
            "You are the MAACO Test Writer. Produce deterministic tests that validate behavior and regressions. Keep fixtures minimal and stable.",
        ["DebuggingAgent"] =
            "You are the MAACO Debugger. Analyze build/test failures, identify root cause, and propose the smallest safe fix with validation steps.",
        ["DocumentationAgent"] =
            "You are the MAACO Documentation Agent. Produce concise developer-facing docs aligned with current implementation and constraints."
    };

    private static readonly IReadOnlyDictionary<string, string> ResponseSchemas = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["TaskPlannerAgent"] = "{\"plan\":[{\"step\":\"string\",\"reason\":\"string\"}],\"risks\":[\"string\"],\"doneCriteria\":[\"string\"]}",
        ["BackendDeveloperAgent"] = "{\"changes\":[{\"file\":\"string\",\"summary\":\"string\"}],\"tests\":[\"string\"],\"notes\":[\"string\"]}",
        ["TestWriterAgent"] = "{\"tests\":[{\"name\":\"string\",\"purpose\":\"string\"}],\"coverageGaps\":[\"string\"]}",
        ["DebuggingAgent"] = "{\"rootCause\":\"string\",\"fixes\":[{\"file\":\"string\",\"change\":\"string\"}],\"validation\":[\"string\"]}",
        ["DocumentationAgent"] = "{\"docs\":[{\"file\":\"string\",\"update\":\"string\"}],\"audience\":\"string\"}"
    };

    public string GetSystemPrompt(string agentName)
        => SystemPrompts.TryGetValue(agentName, out var prompt)
            ? prompt
            : "You are a MAACO agent. Follow safety rules, produce deterministic structured output, and minimize unnecessary changes.";

    public string GetResponseSchema(string agentName)
        => ResponseSchemas.TryGetValue(agentName, out var schema)
            ? schema
            : "{\"result\":\"string\",\"notes\":[\"string\"]}";
}
