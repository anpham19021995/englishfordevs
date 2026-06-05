using System.Text;
using EnglishForDevs.Api.Data;
using EnglishForDevs.Api.Data.Entities;
using EnglishForDevs.Api.Features.Auth;
using EnglishForDevs.Api.Features.Health;
using EnglishForDevs.Api.Features.Me;
using EnglishForDevs.Api.Features.Practice;
using EnglishForDevs.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddInMemoryCollection(LoadDotEnvConfiguration());

var isTesting = builder.Environment.IsEnvironment("Testing");
var databaseConnection = isTesting
    ? ""
    : builder.Configuration.GetConnectionString("DefaultConnection");
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
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "EnglishForDevs",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "EnglishForDevs",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
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

app.UseCors("Frontend");
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

    var mappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["AI_PROVIDER"] = "AI:Provider",
        ["CONNECTIONSTRINGS__DEFAULTCONNECTION"] = "ConnectionStrings:DefaultConnection",
        ["DATABASE_CONNECTION_STRING"] = "ConnectionStrings:DefaultConnection",
        ["OPENAI_API_KEY"] = "OpenAI:ApiKey",
        ["OPENAI_MODEL"] = "OpenAI:Model",
        ["OLLAMA_API_KEY"] = "Ollama:ApiKey",
        ["OLLAMA_BASE_URL"] = "Ollama:BaseUrl",
        ["OLLAMA_MODEL"] = "Ollama:Model",
        ["JWT_SECRET"] = "Jwt:Secret"
    };
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
