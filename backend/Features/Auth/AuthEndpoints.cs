using EnglishForDevs.Api.Services;
using EnglishForDevs.Api.Shared;
using System.Net.Mail;

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
        var email = request.Email.Trim();

        if (string.IsNullOrWhiteSpace(email) || email.Length > ValidationLimits.EmailMaxLength)
        {
            return "A valid email is required.";
        }

        try
        {
            var address = new MailAddress(email);

            if (!string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase))
            {
                return "A valid email is required.";
            }
        }
        catch (FormatException)
        {
            return "A valid email is required.";
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < ValidationLimits.PasswordMinLength)
        {
            return $"Password must be at least {ValidationLimits.PasswordMinLength} characters.";
        }

        if (request.Password.Length > ValidationLimits.PasswordMaxLength)
        {
            return $"Password must be {ValidationLimits.PasswordMaxLength} characters or fewer.";
        }

        return null;
    }
}
