namespace MAACO.Infrastructure.Llm;

internal static class LlmHttpRetry
{
    public static async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> sendAsync,
        int maxRetryCount,
        CancellationToken cancellationToken,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        var attempts = Math.Max(0, maxRetryCount) + 1;
        Exception? lastException = null;
        delayAsync ??= static (delay, token) => Task.Delay(delay, token);

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var response = await sendAsync(cancellationToken);
                if (IsTransient(response.StatusCode) && attempt < attempts)
                {
                    response.Dispose();
                    await delayAsync(GetBackoffDelay(attempt), cancellationToken);
                    continue;
                }

                return response;
            }
            catch (HttpRequestException ex) when (IsValidationFailure(ex))
            {
                throw;
            }
            catch (HttpRequestException ex) when (attempt < attempts)
            {
                lastException = ex;
                await delayAsync(GetBackoffDelay(attempt), cancellationToken);
            }
            catch (TaskCanceledException ex) when (attempt < attempts && !cancellationToken.IsCancellationRequested)
            {
                lastException = ex;
                await delayAsync(GetBackoffDelay(attempt), cancellationToken);
            }
        }

        throw lastException ?? new InvalidOperationException("HTTP request failed after retries.");
    }

    internal static TimeSpan GetBackoffDelay(int attempt)
    {
        var boundedAttempt = Math.Max(1, attempt);
        var multiplier = Math.Pow(2, boundedAttempt - 1);
        var delayMs = Math.Min(100 * multiplier, 5000);
        return TimeSpan.FromMilliseconds(delayMs);
    }

    private static bool IsTransient(System.Net.HttpStatusCode code) =>
        code == System.Net.HttpStatusCode.RequestTimeout ||
        code == System.Net.HttpStatusCode.TooManyRequests ||
        (int)code >= 500;

    private static bool IsValidationFailure(HttpRequestException ex)
    {
        var statusCode = ex.StatusCode;
        return statusCode == System.Net.HttpStatusCode.BadRequest ||
               statusCode == System.Net.HttpStatusCode.UnprocessableEntity;
    }
}
