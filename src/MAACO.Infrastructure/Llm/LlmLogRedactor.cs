using System.Text.RegularExpressions;

namespace MAACO.Infrastructure.Llm;

internal static partial class LlmLogRedactor
{
    [GeneratedRegex("(?i)(api[_-]?key|authorization|token|password)\\s*[:=]\\s*([^\"]\\S*|\".*?\")", RegexOptions.Compiled)]
    private static partial Regex SecretLikePairRegex();

    [GeneratedRegex("(?i)bearer\\s+[A-Za-z0-9\\-_.=]+", RegexOptions.Compiled)]
    private static partial Regex BearerRegex();

    public static string Redact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var masked = SecretLikePairRegex().Replace(value, m => $"{m.Groups[1].Value}=***REDACTED***");
        masked = BearerRegex().Replace(masked, "Bearer ***REDACTED***");
        return masked;
    }
}
