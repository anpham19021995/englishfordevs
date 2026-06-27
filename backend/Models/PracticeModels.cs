namespace EnglishForDevs.Api.Services;

public static class PracticeModes
{
    public const string Chat = "chat";
    public const string Interview = "interview";
    public const string Converter = "converter";

    public static readonly string[] All = [Chat, Interview, Converter];
}

public sealed record PracticeRequest(
    string Mode = PracticeModes.Chat,
    string Message = "");

public sealed record PracticeResponse(
    PracticeFeedback Feedback,
    string Source,
    PracticeHistoryItem? Attempt = null);

public sealed record PracticeHistoryItem(
    Guid Id,
    Guid? UserId,
    string Mode,
    string Message,
    PracticeFeedback Feedback,
    string Source,
    DateTimeOffset CreatedAt);

public sealed record PracticeFeedback(
    string DirectReply,
    string CorrectedVersion,
    string NaturalVersion,
    string[] Vocabulary,
    string ConfidenceFeedback,
    string FollowUpQuestion);

public sealed record TranscribePracticeAudioResponse(string Transcript);

public sealed record TextToSpeechRequest(string Text = "");

public sealed record AuthRequest(
    string Email = "",
    string Password = "");

public sealed record AuthResponse(
    string Token,
    Guid UserId,
    string Email,
    DateTimeOffset ExpiresAt);

public sealed record UserProgressResponse(
    int TotalPractices,
    int ChatPractices,
    int InterviewPractices,
    int ConverterPractices,
    int CurrentStreakDays,
    DateTimeOffset? LastPracticeAt);

public sealed record UserProfileResponse(
    Guid UserId,
    string Email,
    DateTimeOffset? CreatedAt);
