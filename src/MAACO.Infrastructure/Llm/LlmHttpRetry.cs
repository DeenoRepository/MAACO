namespace MAACO.Infrastructure.Llm;

internal static class LlmHttpRetry
{
    public static async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> sendAsync,
        int maxRetryCount,
        CancellationToken cancellationToken)
    {
        var attempts = Math.Max(0, maxRetryCount) + 1;
        Exception? lastException = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var response = await sendAsync(cancellationToken);
                if (IsTransient(response.StatusCode) && attempt < attempts)
                {
                    response.Dispose();
                    await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), cancellationToken);
                    continue;
                }

                return response;
            }
            catch (HttpRequestException ex) when (attempt < attempts)
            {
                lastException = ex;
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), cancellationToken);
            }
            catch (TaskCanceledException ex) when (attempt < attempts && !cancellationToken.IsCancellationRequested)
            {
                lastException = ex;
                await Task.Delay(TimeSpan.FromMilliseconds(100 * attempt), cancellationToken);
            }
        }

        throw lastException ?? new InvalidOperationException("HTTP request failed after retries.");
    }

    private static bool IsTransient(System.Net.HttpStatusCode code) =>
        code == System.Net.HttpStatusCode.RequestTimeout ||
        code == System.Net.HttpStatusCode.TooManyRequests ||
        (int)code >= 500;
}
