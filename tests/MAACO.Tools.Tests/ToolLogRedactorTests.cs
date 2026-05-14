using MAACO.Tools;

namespace MAACO.Tools.Tests;

public sealed class ToolLogRedactorTests
{
    [Fact]
    public void Redact_MasksJsonSecretFields()
    {
        const string input = "{\"apiKey\":\"abc123\",\"password\":\"qwerty\"}";

        var result = ToolLogRedactor.Redact(input);

        Assert.DoesNotContain("abc123", result);
        Assert.DoesNotContain("qwerty", result);
        Assert.Contains("***REDACTED***", result);
    }

    [Fact]
    public void Redact_MasksKeyValueSecrets()
    {
        const string input = "token: xyz-123 password=my-pass";

        var result = ToolLogRedactor.Redact(input);

        Assert.DoesNotContain("xyz-123", result);
        Assert.DoesNotContain("my-pass", result);
        Assert.Contains("token=***REDACTED***", result, StringComparison.OrdinalIgnoreCase);
    }
}
