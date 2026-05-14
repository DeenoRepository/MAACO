using MAACO.Core.Abstractions.Llm;
using MAACO.Infrastructure.Llm;
using System.Net;
using System.Net.Http;
using System.Text;

namespace MAACO.Core.Tests;

public sealed class LlmProvidersHttpTests
{
    [Fact]
    public async Task OpenAiProvider_GenerateAsync_ParsesResponse()
    {
        var handler = new QueueHttpMessageHandler(
        [
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"choices":[{"message":{"content":"ok-openai"}}],"usage":{"prompt_tokens":10,"completion_tokens":5,"total_tokens":15}}""",
                    Encoding.UTF8,
                    "application/json")
            }
        ]);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var provider = new OpenAiCompatibleLlmProvider(
            client,
            new LlmProviderOptions("OpenAI-Compatible", "gpt-5", Timeout: TimeSpan.FromSeconds(2), MaxRetryCount: 1));

        var response = await provider.GenerateAsync(
            new LlmRequest([new LlmMessage(LlmMessageRole.User, "hello")]),
            CancellationToken.None);

        Assert.True(response.Succeeded);
        Assert.Equal("ok-openai", response.Content);
        Assert.Equal(15, response.Usage.TotalTokens);
    }

    [Fact]
    public async Task OllamaProvider_GenerateAsync_RetriesTransientFailure()
    {
        var handler = new QueueHttpMessageHandler(
        [
            new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("busy") },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"message":{"content":"ok-ollama"},"prompt_eval_count":7,"eval_count":3}""",
                    Encoding.UTF8,
                    "application/json")
            }
        ]);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        var provider = new OllamaLlmProvider(
            client,
            new LlmProviderOptions("Ollama", "llama3.1", Timeout: TimeSpan.FromSeconds(2), MaxRetryCount: 2));

        var response = await provider.GenerateAsync(
            new LlmRequest([new LlmMessage(LlmMessageRole.User, "hello")]),
            CancellationToken.None);

        Assert.True(response.Succeeded);
        Assert.Equal("ok-ollama", response.Content);
        Assert.Equal(2, handler.CallCount);
    }

    [Fact]
    public async Task Providers_HealthCheckAsync_ReturnsTrue_OnSuccess()
    {
        var openAiClient = new HttpClient(new QueueHttpMessageHandler(
        [
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"data":[]}""", Encoding.UTF8, "application/json")
            }
        ]))
        { BaseAddress = new Uri("http://localhost") };

        var ollamaClient = new HttpClient(new QueueHttpMessageHandler(
        [
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"models":[]}""", Encoding.UTF8, "application/json")
            }
        ]))
        { BaseAddress = new Uri("http://localhost") };

        var openAi = new OpenAiCompatibleLlmProvider(openAiClient, new LlmProviderOptions("OpenAI-Compatible", "gpt-5"));
        var ollama = new OllamaLlmProvider(ollamaClient, new LlmProviderOptions("Ollama", "llama3.1"));

        Assert.True(await openAi.HealthCheckAsync(CancellationToken.None));
        Assert.True(await ollama.HealthCheckAsync(CancellationToken.None));
    }

    private sealed class QueueHttpMessageHandler(IReadOnlyList<HttpResponseMessage> responses) : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> responseQueue = new(responses);
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            if (responseQueue.Count == 0)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
            }

            return Task.FromResult(responseQueue.Dequeue());
        }
    }
}
