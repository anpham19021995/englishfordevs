namespace EnglishForDevs.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(AuthRequest request, CancellationToken cancellationToken);

    Task<AuthResponse?> LoginAsync(AuthRequest request, CancellationToken cancellationToken);
}
