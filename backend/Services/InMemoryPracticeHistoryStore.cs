using System.Collections.Concurrent;
using EnglishForDevs.Api.Data.Entities;

namespace EnglishForDevs.Api.Services;

public sealed class InMemoryPracticeHistoryStore : IPracticeHistoryStore
{
    private readonly ConcurrentQueue<PracticeHistoryItem> history = new();

    public Task<PracticeHistoryItem> SaveAsync(
        PracticeRequest request,
        PracticeResponse response,
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var item = PracticeAttempt.From(request, response, userId).ToHistoryItem();
        history.Enqueue(item);

        while (history.Count > 100 && history.TryDequeue(out _))
        {
        }

        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<PracticeHistoryItem>> GetRecentAsync(
        Guid? userId,
        int take,
        CancellationToken cancellationToken)
    {
        var items = history
            .Reverse()
            .Where(item => item.UserId == userId)
            .Take(Math.Clamp(take, 1, 50))
            .ToArray();

        return Task.FromResult<IReadOnlyList<PracticeHistoryItem>>(items);
    }

    public Task<int> ClearAsync(
        Guid? userId,
        CancellationToken cancellationToken)
    {
        var items = history.ToArray();
        var kept = items.Where(item => item.UserId != userId).ToArray();
        var removedCount = items.Length - kept.Length;

        while (history.TryDequeue(out _))
        {
        }

        foreach (var item in kept)
        {
            history.Enqueue(item);
        }

        return Task.FromResult(removedCount);
    }
}
