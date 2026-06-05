using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnglishForDevs.Api.Data;
using EnglishForDevs.Api.Data.Entities;
using EnglishForDevs.Api.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EnglishForDevs.Api.Services;

public sealed class AuthService(
    AppDbContext dbContext,
    IConfiguration configuration,
    IHostEnvironment environment,
    PasswordHasher<AppUser> passwordHasher) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(
        AuthRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        if (await dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        var user = new AppUser { Email = email };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreateToken(user);
    }

    public async Task<AuthResponse?> LoginAsync(
        AuthRequest request,
        CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await dbContext.Users.FirstOrDefaultAsync(
            item => item.Email == email,
            cancellationToken);

        if (user is null)
        {
            return null;
        }

        var result = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        return result == PasswordVerificationResult.Failed ? null : CreateToken(user);
    }

    private AuthResponse CreateToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            GetJwtSecret(configuration, environment.IsDevelopment())));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(12);
        var token = new JwtSecurityToken(
            issuer: configuration[ConfigurationKeys.JwtIssuer] ?? ApplicationConstants.Name,
            audience: configuration[ConfigurationKeys.JwtAudience] ?? ApplicationConstants.Name,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            ],
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.Id,
            user.Email,
            expiresAt);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public static string GetJwtSecret(
        IConfiguration configuration,
        bool allowDevelopmentFallback = false)
    {
        var secret = configuration[ConfigurationKeys.JwtSecret];

        if (!string.IsNullOrWhiteSpace(secret))
        {
            return secret;
        }

        return ApplicationConstants.DevelopmentJwtSecret;
    }
}
