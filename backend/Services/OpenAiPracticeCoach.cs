using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EnglishForDevs.Api.Services;

public sealed class OpenAiPracticeCoach(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<OpenAiPracticeCoach> logger) : IOpenAiPracticeCoach
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly Dictionary<string, string> ModePrompts = new()
    {
        [PracticeModes.Chat] =
            "You are an English coach for software engineers practicing workplace conversations. Give feedback on the learner's exact message, not a generic example. Keep the tone supportive, direct, and practical for standups, bug reports, pull requests, and incident discussion.",
        [PracticeModes.Interview] =
            "You are a senior engineering manager coaching a backend developer for technical interviews. Improve the learner's exact answer for clarity, structure, trade-offs, and engineering impact. Keep the tone realistic and interview-focused.",
        [PracticeModes.Converter] =
            "You convert Vietnamese or mixed Vietnamese-English IT explanations into natural, professional English for software work. Preserve the learner's meaning and make it sound clear in a team or client context."
    };

    private const string ResponseContract =
        """
        Return only valid JSON with this shape: {"directReply":"...","correctedVersion":"...","naturalVersion":"...","vocabulary":["..."],"confidenceFeedback":"...","followUpQuestion":"..."}.
        Field rules:
        - directReply: 1-2 natural teammate/interviewer replies to the learner's exact message.
        - correctedVersion: fix grammar while staying close to the learner's original wording.
        - naturalVersion: a polished professional version a developer could say at work.
        - vocabulary: 3-5 strings formatted as "phrase - short meaning or example"; choose phrases useful for software work.
        - confidenceFeedback: 1 specific coaching note about tone, clarity, or confidence; do not use a numeric score.
        - followUpQuestion: 1 question that helps the learner continue the same workplace scenario.
        Do not invent unrelated context. If the learner's message is short, make reasonable minimal assumptions and say what detail would help.
        Keep every field concise and practical.
        """;

    private static readonly object FeedbackSchema = new
    {
        type = "object",
        additionalProperties = false,
        required = new[]
        {
            "directReply",
            "correctedVersion",
            "naturalVersion",
            "vocabulary",
            "confidenceFeedback",
            "followUpQuestion"
        },
        properties = new
        {
            directReply = new
            {
                type = "string",
                description = "One or two natural teammate/interviewer replies to the learner's exact message."
            },
            correctedVersion = new
            {
                type = "string",
                description = "A grammatically corrected version that stays close to the learner's original wording."
            },
            naturalVersion = new
            {
                type = "string",
                description = "A polished professional version a developer could say at work."
            },
            vocabulary = new
            {
                type = "array",
                minItems = 2,
                maxItems = 5,
                items = new
                {
                    type = "string",
                    pattern = ".+ - .+"
                },
                description = "Useful software-work phrases formatted as 'phrase - short meaning or example'."
            },
            confidenceFeedback = new
            {
                type = "string",
                description = "One specific non-numeric coaching note about tone, clarity, or confidence."
            },
            followUpQuestion = new
            {
                type = "string",
                description = "One relevant follow-up question for continued practice."
            }
        }
    };

    private static readonly object FeedbackResponseFormat = new
    {
        type = "json_schema",
        json_schema = new
        {
            name = "practice_feedback",
            strict = true,
            schema = FeedbackSchema
        }
    };

    public async Task<PracticeResponse> GenerateFeedbackAsync(
        PracticeRequest request,
        CancellationToken cancellationToken)
    {
        var provider = configuration["AI:Provider"];

        if (string.Equals(provider, "ollama", StringComparison.OrdinalIgnoreCase))
        {
            return await GenerateOllamaFeedbackAsync(request, cancellationToken);
        }

        var apiKey = configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new PracticeResponse(FallbackFeedback.ForMode(request.Mode), "local-fallback");
        }

        var model = configuration["OpenAI:Model"] ?? "gpt-4o-mini";
        var payload = new
        {
            model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = $"{ModePrompts[request.Mode]} {ResponseContract}"
                },
                new
                {
                    role = "user",
                    content = request.Message.Trim()
                }
            },
            response_format = FeedbackResponseFormat,
            temperature = 0.5
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");

        try
        {
            using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var completion = await response.Content.ReadFromJsonAsync<OpenAiChatCompletion>(
                JsonOptions,
                cancellationToken);
            var content = completion?.Choices.FirstOrDefault()?.Message.Content;
            var feedback = NormalizeFeedback(content, request.Mode);

            return new PracticeResponse(feedback, "openai");
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            logger.LogWarning(exception, "OpenAI feedback generation failed. Returning local fallback.");
            return new PracticeResponse(FallbackFeedback.ForMode(request.Mode), "local-fallback");
        }
    }

    private async Task<PracticeResponse> GenerateOllamaFeedbackAsync(
        PracticeRequest request,
        CancellationToken cancellationToken)
    {
        var apiKey = configuration["Ollama:ApiKey"];
        var baseUrl = (configuration["Ollama:BaseUrl"] ?? "http://localhost:11434/api").TrimEnd('/');
        var model = configuration["Ollama:Model"] ?? "gpt-oss:20b";
        var payload = new
        {
            model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = $"{ModePrompts[request.Mode]} {ResponseContract}"
                },
                new
                {
                    role = "user",
                    content = request.Message.Trim()
                }
            },
            format = FeedbackSchema,
            stream = false,
            options = new
            {
                temperature = 0.5
            }
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}/chat");

        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(payload, JsonOptions),
            Encoding.UTF8,
            "application/json");

        try
        {
            using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
            response.EnsureSuccessStatusCode();

            var completion = await response.Content.ReadFromJsonAsync<OllamaChatCompletion>(
                JsonOptions,
                cancellationToken);
            var feedback = NormalizeFeedback(completion?.Message.Content, request.Mode);

            return new PracticeResponse(feedback, "ollama");
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            logger.LogWarning(exception, "Ollama feedback generation failed. Returning local fallback.");
            return new PracticeResponse(FallbackFeedback.ForMode(request.Mode), "local-fallback");
        }
    }

    private static PracticeFeedback NormalizeFeedback(string? content, string mode)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return FallbackFeedback.ForMode(mode);
        }

        content = StripJsonFence(content);

        try
        {
            var feedback = ParseFeedback(content);
            return string.IsNullOrWhiteSpace(feedback.DirectReply)
                ? FallbackFeedback.ForMode(mode)
                : feedback;
        }
        catch (JsonException)
        {
            var fallback = FallbackFeedback.ForMode(mode);
            return fallback with { DirectReply = content };
        }
    }

    private static string StripJsonFence(string content)
    {
        content = content.Trim();

        if (!content.StartsWith("```", StringComparison.Ordinal))
        {
            return content;
        }

        var firstLineEnd = content.IndexOf('\n');
        var lastFenceStart = content.LastIndexOf("```", StringComparison.Ordinal);

        if (firstLineEnd < 0 || lastFenceStart <= firstLineEnd)
        {
            return content;
        }

        return content[(firstLineEnd + 1)..lastFenceStart].Trim();
    }

    private static PracticeFeedback ParseFeedback(string content)
    {
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        return new PracticeFeedback(
            CleanText(GetRequiredString(root, "directReply"), 420),
            CleanText(GetRequiredString(root, "correctedVersion"), 420),
            CleanText(GetRequiredString(root, "naturalVersion"), 520),
            GetVocabulary(root, "vocabulary"),
            CleanText(GetRequiredString(root, "confidenceFeedback"), 360),
            CleanText(GetRequiredString(root, "followUpQuestion"), 240));
    }

    private static string GetRequiredString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
        {
            throw new JsonException($"Missing required property '{propertyName}'.");
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? "",
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => value.GetRawText()
        };
    }

    private static string[] GetVocabulary(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
        {
            throw new JsonException($"Missing required property '{propertyName}'.");
        }

        if (value.ValueKind != JsonValueKind.Array)
        {
            return [FormatVocabularyItem(GetRequiredString(root, propertyName))];
        }

        return value
            .EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String
                ? item.GetString() ?? ""
                : item.GetRawText())
            .Select(FormatVocabularyItem)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToArray();
    }

    private static string CleanText(string value, int maxLength)
    {
        value = value
            .Replace("```json", "", StringComparison.OrdinalIgnoreCase)
            .Replace("```", "")
            .Trim();

        if (value.Length <= maxLength)
        {
            return value;
        }

        return $"{value[..Math.Max(0, maxLength - 3)].TrimEnd()}...";
    }

    private static string FormatVocabularyItem(string value)
    {
        value = CleanText(value, 120);

        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        return value.Contains(" - ", StringComparison.Ordinal)
            ? value
            : $"{value} - useful phrase for this context";
    }

    private sealed record OpenAiChatCompletion(OpenAiChoice[] Choices);

    private sealed record OpenAiChoice(OpenAiMessage Message);

    private sealed record OpenAiMessage(string Content);

    private sealed record OllamaChatCompletion(OllamaMessage Message);

    private sealed record OllamaMessage(string Content);
}
