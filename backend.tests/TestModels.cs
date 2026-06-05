using System.Text.Json.Serialization;

namespace EnglishForDevs.Api.Tests;

public sealed record AuthResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("userId")] Guid UserId,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("expiresAt")] DateTimeOffset ExpiresAt);

public sealed record PracticeResponse(
    [property: JsonPropertyName("feedback")] PracticeFeedback Feedback,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("attempt")] PracticeHistoryItem? Attempt);

public sealed record PracticeHistoryItem(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("userId")] Guid? UserId,
    [property: JsonPropertyName("mode")] string Mode,
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("feedback")] PracticeFeedback Feedback,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt);

public sealed record PracticeFeedback(
    [property: JsonPropertyName("directReply")] string DirectReply,
    [property: JsonPropertyName("correctedVersion")] string CorrectedVersion,
    [property: JsonPropertyName("naturalVersion")] string NaturalVersion,
    [property: JsonPropertyName("vocabulary")] string[] Vocabulary,
    [property: JsonPropertyName("confidenceFeedback")] string ConfidenceFeedback,
    [property: JsonPropertyName("followUpQuestion")] string FollowUpQuestion);

public sealed record UserProgressResponse(
    [property: JsonPropertyName("totalPractices")] int TotalPractices,
    [property: JsonPropertyName("chatPractices")] int ChatPractices,
    [property: JsonPropertyName("interviewPractices")] int InterviewPractices,
    [property: JsonPropertyName("converterPractices")] int ConverterPractices,
    [property: JsonPropertyName("currentStreakDays")] int CurrentStreakDays,
    [property: JsonPropertyName("lastPracticeAt")] DateTimeOffset? LastPracticeAt);

public sealed record ErrorResponse(
    [property: JsonPropertyName("error")] string Error);
