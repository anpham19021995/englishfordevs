using System.Security.Claims;
using EnglishForDevs.Api.Data;
using EnglishForDevs.Api.Services;
using EnglishForDevs.Api.Shared;
using Microsoft.EntityFrameworkCore;

namespace EnglishForDevs.Api.Features.Me;

public static class GetMyProfile
{
    public static async Task<IResult> HandleAsync(
        IServiceProvider serviceProvider,
        ClaimsPrincipal user,
        CancellationToken cancellationToken)
    {
        var userId = CurrentUser.GetId(user);
        var dbContext = serviceProvider.GetService<AppDbContext>();

        if (dbContext is not null)
        {
            var profile = await dbContext.Users
                .AsNoTracking()
                .Where(item => item.Id == userId)
                .Select(item => new UserProfileResponse(
                    item.Id,
                    item.Email,
                    item.CreatedAt))
                .FirstOrDefaultAsync(cancellationToken);

            return profile is null ? Results.NotFound() : Results.Ok(profile);
        }

        return Results.Ok(new UserProfileResponse(
            userId,
            CurrentUser.GetEmail(user),
            null));
    }
}
