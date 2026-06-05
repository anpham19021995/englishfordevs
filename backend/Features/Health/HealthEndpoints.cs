namespace EnglishForDevs.Api.Features.Health;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/health", () => Results.Ok(new
        {
            status = "ok",
            app = "EnglishForDevs.Api",
            timestamp = DateTimeOffset.UtcNow
        }))
        .WithName("HealthCheck");

        endpoints.MapGet("/api/health/ai", (
            IConfiguration configuration,
            IWebHostEnvironment environment) => Results.Ok(new
            {
                environment = environment.EnvironmentName,
                historyStorage = string.IsNullOrWhiteSpace(
                    configuration.GetConnectionString("DefaultConnection"))
                    ? "in-memory"
                    : "postgres",
                databaseConfigured = !string.IsNullOrWhiteSpace(
                    configuration.GetConnectionString("DefaultConnection")),
                jwtSecretConfigured = !string.IsNullOrWhiteSpace(configuration["Jwt:Secret"]),
                provider = configuration["AI:Provider"] ?? "",
                openAiApiKeyConfigured = !string.IsNullOrWhiteSpace(configuration["OpenAI:ApiKey"]),
                ollamaApiKeyConfigured = !string.IsNullOrWhiteSpace(configuration["Ollama:ApiKey"]),
                ollamaBaseUrl = configuration["Ollama:BaseUrl"] ?? "",
                ollamaModel = configuration["Ollama:Model"] ?? ""
            }))
        .WithName("AiConfigHealthCheck");

        return endpoints;
    }
}
