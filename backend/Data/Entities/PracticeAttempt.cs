using EnglishForDevs.Api.Services;

namespace EnglishForDevs.Api.Data.Entities;

public sealed class PracticeAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public AppUser? User { get; set; }
    public string Mode { get; set; } = PracticeModes.Chat;
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string DirectReply { get; set; } = string.Empty;
    public string CorrectedVersion { get; set; } = string.Empty;
    public string NaturalVersion { get; set; } = string.Empty;
    public string[] Vocabulary { get; set; } = [];
    public string ConfidenceFeedback { get; set; } = string.Empty;
    public string FollowUpQuestion { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public PracticeHistoryItem ToHistoryItem()
    {
        return new PracticeHistoryItem(
            Id,
            UserId,
            Mode,
            Message,
            new PracticeFeedback(
                DirectReply,
                CorrectedVersion,
                NaturalVersion,
                Vocabulary,
                ConfidenceFeedback,
                FollowUpQuestion),
            Source,
            CreatedAt);
    }

    public static PracticeAttempt From(
        PracticeRequest request,
        PracticeResponse response,
        Guid? userId)
    {
        return new PracticeAttempt
        {
            UserId = userId,
            Mode = request.Mode,
            Message = request.Message.Trim(),
            Source = response.Source,
            DirectReply = response.Feedback.DirectReply,
            CorrectedVersion = response.Feedback.CorrectedVersion,
            NaturalVersion = response.Feedback.NaturalVersion,
            Vocabulary = response.Feedback.Vocabulary,
            ConfidenceFeedback = response.Feedback.ConfidenceFeedback,
            FollowUpQuestion = response.Feedback.FollowUpQuestion
        };
    }
}
