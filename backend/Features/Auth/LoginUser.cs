using EnglishForDevs.Api.Services;

namespace EnglishForDevs.Api.Features.Auth;

public static class LoginUser
{
    public static async Task<IResult> HandleAsync(
        AuthRequest request,
        IAuthService authService,
        CancellationToken cancellationToken)
    {
        var validationError = AuthEndpoints.Validate(request);
        if (validationError is not null)
        {
            return Results.BadRequest(new { error = validationError });
        }

        var response = await authService.LoginAsync(request, cancellationToken);
        return response is null
            ? Results.Unauthorized()
            : Results.Ok(response);
    }
}
