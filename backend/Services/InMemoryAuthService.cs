using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EnglishForDevs.Api.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace EnglishForDevs.Api.Services;

public sealed class InMemoryAuthService(
    IConfiguration configuration,
    IHostEnvironment environment,
    PasswordHasher<AppUser> passwordHasher) : IAuthService
{
    private readonly ConcurrentDictionary<string, AppUser> users = new();

    public Task<AuthResponse> RegisterAsync(
        AuthRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = new AppUser { Email = email };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        if (!users.TryAdd(email, user))
        {
            throw new InvalidOperationException("Email is already registered.");
        }

        return Task.FromResult(CreateToken(user));
    }

    public Task<AuthResponse?> LoginAsync(
        AuthRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (!users.TryGetValue(email, out var user))
        {
            return Task.FromResult<AuthResponse?>(null);
        }

        var result = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        return Task.FromResult<AuthResponse?>(
            result == PasswordVerificationResult.Failed ? null : CreateToken(user));
    }

    private AuthResponse CreateToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            AuthService.GetJwtSecret(configuration, environment.IsDevelopment())));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTimeOffset.UtcNow.AddHours(12);
        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"] ?? "EnglishForDevs",
            audience: configuration["Jwt:Audience"] ?? "EnglishForDevs",
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
}
