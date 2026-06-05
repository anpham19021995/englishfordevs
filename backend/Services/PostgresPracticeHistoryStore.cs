using EnglishForDevs.Api.Data;
using EnglishForDevs.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishForDevs.Api.Services;

public sealed class PostgresPracticeHistoryStore(AppDbContext dbContext) : IPracticeHistoryStore
{
    public async Task<PracticeHistoryItem> SaveAsync(
        PracticeRequest request,
        PracticeResponse response,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var attempt = PracticeAttempt.From(request, response, userId);

        dbContext.PracticeAttempts.Add(attempt);
        await dbContext.SaveChangesAsync(cancellationToken);

        return attempt.ToHistoryItem();
    }

    public async Task<IReadOnlyList<PracticeHistoryItem>> GetRecentAsync(
        Guid? userId,
        int take,
        CancellationToken cancellationToken)
    {
        var attempts = await dbContext.PracticeAttempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId)
            .OrderByDescending(attempt => attempt.CreatedAt)
            .Take(Math.Clamp(take, 1, 50))
            .ToListAsync(cancellationToken);

        return attempts.Select(attempt => attempt.ToHistoryItem()).ToList();
    }

    public Task<int> ClearAsync(
        Guid? userId,
        CancellationToken cancellationToken)
    {
        return dbContext.PracticeAttempts
            .Where(attempt => attempt.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
