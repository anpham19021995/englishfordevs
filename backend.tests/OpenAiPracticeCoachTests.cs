using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EnglishForDevs.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnglishForDevs.Api.Tests;

public sealed class OpenAiPracticeCoachTests
{
    [Fact]
    public async Task GenerateFeedbackAsync_UsesStructuredOutputsAndParsesFeedback()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = "test-api-key",
                ["OpenAI:Model"] = "gpt-4o-mini"
            })
            .Build();
        var coach = new OpenAiPracticeCoach(
            httpClient,
            configuration,
            NullLogger<OpenAiPracticeCoach>.Instance);

        var response = await coach.GenerateFeedbackAsync(
            new PracticeRequest(PracticeModes.Chat, "I fixed bug in API."),
            CancellationToken.None);

        Assert.Equal("openai", response.Source);
        Assert.Equal("Nice work. Tell me what changed.", response.Feedback.DirectReply);
        Assert.Equal("Bearer", handler.Request?.Headers.Authorization?.Scheme);
        Assert.Equal("test-api-key", handler.Request?.Headers.Authorization?.Parameter);

        using var payload = JsonDocument.Parse(handler.RequestBody);
        Assert.Equal("gpt-4o-mini", payload.RootElement.GetProperty("model").GetString());
        var systemPrompt = payload.RootElement
            .GetProperty("messages")[0]
            .GetProperty("content")
            .GetString();
        Assert.Contains("learner's exact message", systemPrompt);
        Assert.Contains("phrase - short meaning or example", systemPrompt);
        var responseFormat = payload.RootElement.GetProperty("response_format");
        Assert.Equal("json_schema", responseFormat.GetProperty("type").GetString());
        Assert.True(responseFormat.GetProperty("json_schema").GetProperty("strict").GetBoolean());
        Assert.Equal(
            "practice_feedback",
            responseFormat.GetProperty("json_schema").GetProperty("name").GetString());
    }

    [Fact]
    public async Task GenerateFeedbackAsync_CanUseOllamaProvider()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AI:Provider"] = "ollama",
                ["Ollama:ApiKey"] = "test-ollama-key",
                ["Ollama:BaseUrl"] = "https://ollama.com/api",
                ["Ollama:Model"] = "gpt-oss:20b"
            })
            .Build();
        var coach = new OpenAiPracticeCoach(
            httpClient,
            configuration,
            NullLogger<OpenAiPracticeCoach>.Instance);

        var response = await coach.GenerateFeedbackAsync(
            new PracticeRequest(PracticeModes.Chat, "I fixed bug in API."),
            CancellationToken.None);

        Assert.Equal("ollama", response.Source);
        Assert.Equal("Nice work. Tell me what changed.", response.Feedback.DirectReply);
        Assert.Equal("https://ollama.com/api/chat", handler.Request?.RequestUri?.ToString());
        Assert.Equal("Bearer", handler.Request?.Headers.Authorization?.Scheme);
        Assert.Equal("test-ollama-key", handler.Request?.Headers.Authorization?.Parameter);

        using var payload = JsonDocument.Parse(handler.RequestBody);
        Assert.Equal("gpt-oss:20b", payload.RootElement.GetProperty("model").GetString());
        Assert.False(payload.RootElement.GetProperty("stream").GetBoolean());
        Assert.Equal("object", payload.RootElement.GetProperty("format").GetProperty("type").GetString());
    }

    [Fact]
    public async Task GenerateFeedbackAsync_NormalizesOllamaMarkdownJson()
    {
        using var handler = new CapturingHandler(
            """
            ```json
            {
              "directReply": "Great work.",
              "correctedVersion": "I fixed a bug in the API.",
              "naturalVersion": "I fixed an API bug and verified the behavior.",
              "vocabulary": ["verified", "endpoint", "regression", "release", "monitoring", "extra"],
              "confidenceFeedback": 0.85,
              "followUpQuestion": "How did you test it?"
            }
            ```
            """);
        using var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AI:Provider"] = "ollama",
                ["Ollama:ApiKey"] = "test-ollama-key",
                ["Ollama:BaseUrl"] = "https://ollama.com/api",
                ["Ollama:Model"] = "gpt-oss:20b"
            })
            .Build();
        var coach = new OpenAiPracticeCoach(
            httpClient,
            configuration,
            NullLogger<OpenAiPracticeCoach>.Instance);

        var response = await coach.GenerateFeedbackAsync(
            new PracticeRequest(PracticeModes.Chat, "I fixed bug in API."),
            CancellationToken.None);

        Assert.Equal("ollama", response.Source);
        Assert.Equal("Great work.", response.Feedback.DirectReply);
        Assert.Equal("0.85", response.Feedback.ConfidenceFeedback);
        Assert.Equal(5, response.Feedback.Vocabulary.Length);
        Assert.All(response.Feedback.Vocabulary, item => Assert.Contains(" - ", item));
    }

    private sealed class CapturingHandler(string? responseContent = null) : HttpMessageHandler, IDisposable
    {
        public HttpRequestMessage? Request { get; private set; }
        public string RequestBody { get; private set; } = "";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = request.Content is null
                ? ""
                : await request.Content.ReadAsStringAsync(cancellationToken);

            var feedback = responseContent ?? """
                {
                  "directReply": "Nice work. Tell me what changed.",
                  "correctedVersion": "I fixed a bug in the API.",
                  "naturalVersion": "I fixed an API bug and verified the endpoint.",
                  "vocabulary": ["verified - confirmed the fix works", "endpoint - an API URL/action"],
                  "confidenceFeedback": "Clear and direct.",
                  "followUpQuestion": "How did you test the fix?"
                }
                """;
            object bodyObject = request.RequestUri?.AbsolutePath.EndsWith("/api/chat") == true
                ? new
                {
                    message = new
                    {
                        content = feedback
                    }
                }
                : new
                {
                    choices = new[]
                    {
                        new
                        {
                            message = new
                            {
                                content = feedback
                            }
                        }
                    }
                };
            var body = JsonSerializer.Serialize(bodyObject);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }

        public new void Dispose()
        {
            Request?.Dispose();
            base.Dispose();
        }
    }
}
