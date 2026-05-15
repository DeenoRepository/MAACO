using MAACO.Infrastructure.Llm;
using System.Net;

namespace MAACO.Core.Tests;

public sealed class LlmHttpRetryTests
{
    [Fact]
    public void GetBackoffDelay_ReturnsExponentialSequence()
    {
        Assert.Equal(TimeSpan.FromMilliseconds(100), LlmHttpRetry.GetBackoffDelay(1));
        Assert.Equal(TimeSpan.FromMilliseconds(200), LlmHttpRetry.GetBackoffDelay(2));
        Assert.Equal(TimeSpan.FromMilliseconds(400), LlmHttpRetry.GetBackoffDelay(3));
        Assert.Equal(TimeSpan.FromMilliseconds(800), LlmHttpRetry.GetBackoffDelay(4));
    }

    [Fact]
    public async Task SendWithRetryAsync_RetriesTransientHttpStatus_UntilSuccess()
    {
        var attempts = 0;
        var delays = new List<TimeSpan>();

        var response = await LlmHttpRetry.SendWithRetryAsync(
            async _ =>
            {
                attempts++;
                if (attempts < 3)
                {
                    return new HttpResponseMessage(HttpStatusCode.TooManyRequests);
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            },
            maxRetryCount: 3,
            cancellationToken: CancellationToken.None,
            delayAsync: (delay, _) =>
            {
                delays.Add(delay);
                return Task.CompletedTask;
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, attempts);
        Assert.Equal(new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200) }, delays);
    }

    [Fact]
    public async Task SendWithRetryAsync_DoesNotRetryValidationHttpRequestException()
    {
        var attempts = 0;

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            LlmHttpRetry.SendWithRetryAsync(
                _ =>
                {
                    attempts++;
                    throw new HttpRequestException(
                        "validation failed",
                        null,
                        HttpStatusCode.BadRequest);
                },
                maxRetryCount: 3,
                cancellationToken: CancellationToken.None,
                delayAsync: (_, _) => Task.CompletedTask));

        Assert.Equal(1, attempts);
    }
}
