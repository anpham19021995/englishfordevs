using System.Security.Claims;
using EnglishForDevs.Api.Services;
using EnglishForDevs.Api.Shared;

namespace EnglishForDevs.Api.Features.Practice;

public static class GetPracticeHistory
{
    public static async Task<IResult> HandleAsync(
        IPracticeHistoryStore historyStore,
        ClaimsPrincipal user,
        int? take,
        CancellationToken cancellationToken)
    {
        var history = await historyStore.GetRecentAsync(
            CurrentUser.GetId(user),
            take ?? 20,
            cancellationToken);

        return Results.Ok(history);
    }
}
