namespace EnglishForDevs.Api.Services;

public interface IOpenAiPracticeCoach
{
    Task<PracticeResponse> GenerateFeedbackAsync(
        PracticeRequest request,
        CancellationToken cancellationToken);
}
