using EnglishForDevs.Api.Services;

namespace EnglishForDevs.Api.Features.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth");

        group.MapPost("/register", RegisterUser.HandleAsync)
            .WithName("Register");

        group.MapPost("/login", LoginUser.HandleAsync)
            .WithName("Login");

        return endpoints;
    }

    internal static string? Validate(AuthRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
        {
            return "A valid email is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return "Password must be at least 8 characters.";
        }

        return null;
    }
}
