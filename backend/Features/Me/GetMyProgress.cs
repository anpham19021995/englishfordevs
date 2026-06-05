using System.Security.Claims;
using EnglishForDevs.Api.Data;
using EnglishForDevs.Api.Services;
using EnglishForDevs.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace EnglishForDevs.Api.Features.Me;

public static class GetMyProgress
{
    public static async Task<IResult> HandleAsync(
        IServiceProvider serviceProvider,
        IPracticeHistoryStore historyStore,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUser.GetId(user);
        var dbContext = serviceProvider.GetService<AppDbContext>();

        if (dbContext is not null)
        {
            var attempts = await dbContext.PracticeAttempts
                .AsNoTracking()
                .Where(attempt => attempt.UserId == userId)
                .Select(attempt => new ProgressAttempt(attempt.Mode, attempt.CreatedAt))
                .ToListAsync(cancellationToken);

            return Results.Ok(CreateResponse(attempts));
        }

        var history = await historyStore.GetRecentAsync(userId, 50, cancellationToken);
        return Results.Ok(CreateResponse(
            history.Select(item => new ProgressAttempt(item.Mode, item.CreatedAt))));
    }

    private static UserProgressResponse CreateResponse(IEnumerable<ProgressAttempt> attempts)
    {
        var items = attempts.ToArray();

        return new UserProgressResponse(
            items.Length,
            items.Count(item => item.Mode == PracticeModes.Chat),
            items.Count(item => item.Mode == PracticeModes.Interview),
            items.Count(item => item.Mode == PracticeModes.Converter),
            CalculateStreak(items.Select(item => item.CreatedAt)),
            items.OrderByDescending(item => item.CreatedAt).FirstOrDefault()?.CreatedAt);
    }

    private static int CalculateStreak(IEnumerable<DateTimeOffset> timestamps)
    {
        var practiceDays = timestamps
            .Select(timestamp => timestamp.UtcDateTime.Date)
            .Distinct()
            .ToHashSet();

        if (practiceDays.Count == 0)
        {
            return 0;
        }

        var cursor = DateTime.UtcNow.Date;
        if (!practiceDays.Contains(cursor))
        {
            cursor = cursor.AddDays(-1);
        }

        var streak = 0;
        while (practiceDays.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    private sealed record ProgressAttempt(string Mode, DateTimeOffset CreatedAt);
}
