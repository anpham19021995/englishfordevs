namespace EnglishForDevs.Api.Features.Me;

public static class MeEndpoints
{
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/me")
            .RequireAuthorization();

        group.MapGet("/", GetMyProfile.HandleAsync)
            .WithName("GetMyProfile");

        group.MapGet("/progress", GetMyProgress.HandleAsync)
            .WithName("GetMyProgress");

        return endpoints;
    }
}
