using System.Text.RegularExpressions;

namespace MAACO.Tools;

public static class ToolLogRedactor
{
    private static readonly Regex JsonSecretRegex = new(
        "(\"(?:apiKey|api_key|token|accessToken|access_token|password|secret)\"\\s*:\\s*\")([^\"]*)(\")",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex KeyValueSecretRegex = new(
        "\\b(api[_-]?key|access[_-]?token|token|password|secret)\\b\\s*[:=]\\s*([^\\s,;]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string Redact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var redacted = JsonSecretRegex.Replace(value, "$1***REDACTED***$3");
        redacted = KeyValueSecretRegex.Replace(redacted, "$1=***REDACTED***");
        return redacted;
    }
}
