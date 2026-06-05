using System.Security.Claims;

namespace EnglishForDevs.Api.Shared;

public static class CurrentUser
{
    public static Guid GetId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }

    public static string GetEmail(ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    }
}
