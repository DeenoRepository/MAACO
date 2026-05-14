using System.Text.Json;

using MAACO.Agents.Abstractions;

namespace MAACO.Agents.Services;

internal static class AgentInputSafetyGuard
{
    private static readonly HashSet<string> DangerousOperations = new(StringComparer.OrdinalIgnoreCase)
    {
        "write",
        "delete",
        "move"
    };

    public static string? Validate(AgentContext context)
    {
        if (context.Inputs is null || context.Inputs.Count == 0)
        {
            return null;
        }

        if (context.Inputs.ContainsKey("toolName"))
        {
            return null;
        }

        if (context.Inputs.TryGetValue("operation", out var operation)
            && DangerousOperations.Contains(operation))
        {
            return "Direct file mutation in agent context is forbidden. Use tools layer.";
        }

        if (context.Inputs.ContainsKey("filePath")
            || context.Inputs.ContainsKey("path")
            || context.Inputs.ContainsKey("content"))
        {
            return "Direct file access payload is forbidden in agent context. Use tools layer.";
        }

        if (context.Inputs.TryGetValue("payload", out var payload)
            && TryHasDangerousPayload(payload))
        {
            return "Direct file mutation payload is forbidden in agent context. Use tools layer.";
        }

        return null;
    }

    private static bool TryHasDangerousPayload(string payload)
    {
        try
        {
            using var json = JsonDocument.Parse(payload);
            var root = json.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (root.TryGetProperty("operation", out var operationNode)
                && operationNode.ValueKind == JsonValueKind.String
                && DangerousOperations.Contains(operationNode.GetString() ?? string.Empty))
            {
                return true;
            }

            return root.TryGetProperty("path", out _)
                   || root.TryGetProperty("filePath", out _)
                   || root.TryGetProperty("content", out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
