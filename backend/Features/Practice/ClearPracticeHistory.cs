using System.Security.Claims;
using EnglishForDevs.Api.Services;
using EnglishForDevs.Api.Shared;

namespace EnglishForDevs.Api.Features.Practice;

public static class ClearPracticeHistory
{
    public static async Task<IResult> HandleAsync(
        IPracticeHistoryStore historyStore,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var deletedCount = await historyStore.ClearAsync(
            CurrentUser.GetId(user),
            cancellationToken);

        return Results.Ok(new { deletedCount });
    }
}
