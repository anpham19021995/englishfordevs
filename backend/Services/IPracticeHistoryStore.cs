namespace EnglishForDevs.Api.Services;

public interface IPracticeHistoryStore
{
    Task<PracticeHistoryItem> SaveAsync(
        PracticeRequest request,
        PracticeResponse response,
        Guid? userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<PracticeHistoryItem>> GetRecentAsync(
        Guid? userId,
        int take,
        CancellationToken cancellationToken);

    Task<int> ClearAsync(
        Guid? userId,
        CancellationToken cancellationToken);
}
