namespace EnglishForDevs.Api.Features.Practice;

public static class PracticeEndpoints
{
    public static IEndpointRouteBuilder MapPracticeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/practice")
            .RequireAuthorization();

        group.MapPost("/", GeneratePracticeFeedback.HandleAsync)
            .WithName("PracticeFeedback");

        group.MapGet("/history", GetPracticeHistory.HandleAsync)
            .WithName("PracticeHistory");

        group.MapDelete("/history", ClearPracticeHistory.HandleAsync)
            .WithName("ClearPracticeHistory");

        return endpoints;
    }
}
