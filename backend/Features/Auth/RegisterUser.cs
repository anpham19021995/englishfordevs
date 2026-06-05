using EnglishForDevs.Api.Services;

namespace EnglishForDevs.Api.Features.Auth;

public static class RegisterUser
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

        try
        {
            var response = await authService.RegisterAsync(request, cancellationToken);
            return Results.Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    }
}
