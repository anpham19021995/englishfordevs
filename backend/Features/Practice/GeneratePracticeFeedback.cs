using System.Security.Claims;
using EnglishForDevs.Api.Services;
using EnglishForDevs.Api.Shared;

namespace EnglishForDevs.Api.Features.Practice;

public static class GeneratePracticeFeedback
{
    public static async Task<IResult> HandleAsync(
        PracticeRequest request,
        IOpenAiPracticeCoach coach,
        IPracticeHistoryStore historyStore,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return Results.BadRequest(new { error = "Message is required." });
        }

        if (!PracticeModes.All.Contains(request.Mode))
        {
            return Results.BadRequest(new { error = "Unsupported mode." });
        }

        var response = await coach.GenerateFeedbackAsync(request, cancellationToken);
        var attempt = await historyStore.SaveAsync(
            request,
            response,
            CurrentUser.GetId(user),
            cancellationToken);

        return Results.Ok(response with { Attempt = attempt });
    }
}
