using System.Text;
using EnglishForDevs.Api.Data;
using EnglishForDevs.Api.Data.Entities;
using EnglishForDevs.Api.Features.Auth;
using EnglishForDevs.Api.Features.Health;
using EnglishForDevs.Api.Features.Me;
using EnglishForDevs.Api.Features.Practice;
using EnglishForDevs.Api.Services;
using EnglishForDevs.Api.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddInMemoryCollection(LoadDotEnvConfiguration());
builder.Configuration.AddInMemoryCollection(LoadMappedEnvironmentConfiguration());

var isTesting = builder.Environment.IsEnvironment("Testing");
var databaseConnection = isTesting
    ? ""
    : builder.Configuration.GetConnectionString(ConfigurationKeys.DefaultConnectionName);
var jwtSecret = AuthService.GetJwtSecret(
    builder.Configuration,
    builder.Environment.IsDevelopment() || isTesting);

builder.Services.AddOpenApi();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration[ConfigurationKeys.JwtIssuer] ?? ApplicationConstants.Name,
            ValidAudience = builder.Configuration[ConfigurationKeys.JwtAudience] ?? ApplicationConstants.Name,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy(ApplicationConstants.FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins(GetAllowedOrigins(builder.Configuration))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddHttpClient<IOpenAiPracticeCoach, OpenAiPracticeCoach>();
builder.Services.AddScoped<PasswordHasher<AppUser>>();

if (string.IsNullOrWhiteSpace(databaseConnection))
{
    builder.Services.AddSingleton<IPracticeHistoryStore, InMemoryPracticeHistoryStore>();
    builder.Services.AddSingleton<IAuthService, InMemoryAuthService>();
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(databaseConnection));
    builder.Services.AddScoped<IPracticeHistoryStore, PostgresPracticeHistoryStore>();
    builder.Services.AddScoped<IAuthService, AuthService>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!string.IsNullOrWhiteSpace(databaseConnection))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(ApplicationConstants.FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapMeEndpoints();
app.MapPracticeEndpoints();

app.Run();

static Dictionary<string, string?> LoadDotEnvConfiguration()
{
    var path = FindDotEnvFile();

    if (path is null)
    {
        return [];
    }

    var mappings = GetConfigurationMappings();
    var values = new Dictionary<string, string?>();

    foreach (var line in File.ReadLines(path))
    {
        var trimmed = line.Trim();

        if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#'))
        {
            continue;
        }

        var separatorIndex = trimmed.IndexOf('=');

        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = trimmed[..separatorIndex].Trim();
        var value = trimmed[(separatorIndex + 1)..].Trim().Trim('"', '\'');

        if (!mappings.TryGetValue(key, out var configurationKey) ||
            string.IsNullOrWhiteSpace(value))
        {
            continue;
        }

        values[configurationKey] = value;
    }

    return values;
}

static Dictionary<string, string?> LoadMappedEnvironmentConfiguration()
{
    var values = new Dictionary<string, string?>();

    foreach (var (environmentKey, configurationKey) in GetConfigurationMappings())
    {
        var value = Environment.GetEnvironmentVariable(environmentKey);

        if (!string.IsNullOrWhiteSpace(value))
        {
            values[configurationKey] = value;
        }
    }

    return values;
}

static Dictionary<string, string> GetConfigurationMappings()
{
    return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [EnvironmentVariableNames.AiProvider] = ConfigurationKeys.AiProvider,
        [EnvironmentVariableNames.ConnectionStringsDefaultConnection] = ConfigurationKeys.DefaultConnectionPath,
        [EnvironmentVariableNames.DatabaseConnectionString] = ConfigurationKeys.DefaultConnectionPath,
        [EnvironmentVariableNames.OpenAiApiKey] = ConfigurationKeys.OpenAiApiKey,
        [EnvironmentVariableNames.OpenAiModel] = ConfigurationKeys.OpenAiModel,
        [EnvironmentVariableNames.OllamaApiKey] = ConfigurationKeys.OllamaApiKey,
        [EnvironmentVariableNames.OllamaBaseUrl] = ConfigurationKeys.OllamaBaseUrl,
        [EnvironmentVariableNames.OllamaModel] = ConfigurationKeys.OllamaModel,
        [EnvironmentVariableNames.JwtSecret] = ConfigurationKeys.JwtSecret,
        [EnvironmentVariableNames.CorsAllowedOrigins] = ConfigurationKeys.CorsAllowedOrigins
    };
}

static string[] GetAllowedOrigins(IConfiguration configuration)
{
    var configuredOrigins = configuration[ConfigurationKeys.CorsAllowedOrigins];

    if (string.IsNullOrWhiteSpace(configuredOrigins))
    {
        return ["http://localhost:3000", "http://127.0.0.1:3000"];
    }

    return configuredOrigins
        .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Where(origin => Uri.TryCreate(origin, UriKind.Absolute, out _))
        .DefaultIfEmpty("http://localhost:3000")
        .ToArray();
}

static string? FindDotEnvFile()
{
    foreach (var startPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        var directory = new DirectoryInfo(startPath);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, ".env");

            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }
    }

    return null;
}

public partial class Program;
