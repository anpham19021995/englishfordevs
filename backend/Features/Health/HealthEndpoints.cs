using EnglishForDevs.Api.Shared;

namespace EnglishForDevs.Api.Features.Health;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/health", () => Results.Ok(new
        {
            status = "ok",
            app = ApplicationConstants.ApiName,
            timestamp = DateTimeOffset.UtcNow
        }))
        .WithName("HealthCheck");

        endpoints.MapGet("/api/health/ai", (
            IConfiguration configuration,
            IWebHostEnvironment environment) =>
        {
            var databaseConnection = DatabaseConnection.GetConfiguredConnectionString(configuration);

            return Results.Ok(new
            {
                environment = environment.EnvironmentName,
                historyStorage = string.IsNullOrWhiteSpace(databaseConnection)
                    ? HistoryStorageTypes.InMemory
                    : HistoryStorageTypes.Postgres,
                databaseConfigured = !string.IsNullOrWhiteSpace(databaseConnection),
                jwtSecretConfigured = !string.IsNullOrWhiteSpace(configuration[ConfigurationKeys.JwtSecret]),
                provider = configuration[ConfigurationKeys.AiProvider] ?? "",
                openAiApiKeyConfigured = !string.IsNullOrWhiteSpace(configuration[ConfigurationKeys.OpenAiApiKey]),
                ollamaApiKeyConfigured = !string.IsNullOrWhiteSpace(configuration[ConfigurationKeys.OllamaApiKey]),
                ollamaBaseUrl = configuration[ConfigurationKeys.OllamaBaseUrl] ?? "",
                ollamaModel = configuration[ConfigurationKeys.OllamaModel] ?? "",
                azureSpeechConfigured =
                    !string.IsNullOrWhiteSpace(configuration[ConfigurationKeys.AzureSpeechKey]) &&
                    !string.IsNullOrWhiteSpace(configuration[ConfigurationKeys.AzureSpeechRegion]),
                azureSpeechRegion = configuration[ConfigurationKeys.AzureSpeechRegion] ?? "",
                azureSpeechVoice = configuration[ConfigurationKeys.AzureSpeechVoice] ?? ""
            });
        })
        .WithName("AiConfigHealthCheck");

        return endpoints;
    }
}
