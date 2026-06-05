namespace EnglishForDevs.Api.Shared;

public static class ConfigurationKeys
{
    public const string AiProvider = "AI:Provider";
    public const string DefaultConnectionName = "DefaultConnection";
    public const string DefaultConnectionPath = "ConnectionStrings:DefaultConnection";
    public const string JwtSecret = "Jwt:Secret";
    public const string JwtIssuer = "Jwt:Issuer";
    public const string JwtAudience = "Jwt:Audience";
    public const string OpenAiApiKey = "OpenAI:ApiKey";
    public const string OpenAiModel = "OpenAI:Model";
    public const string OllamaApiKey = "Ollama:ApiKey";
    public const string OllamaBaseUrl = "Ollama:BaseUrl";
    public const string OllamaModel = "Ollama:Model";
    public const string CorsAllowedOrigins = "Cors:AllowedOrigins";
}

public static class EnvironmentVariableNames
{
    public const string AiProvider = "AI_PROVIDER";
    public const string ConnectionStringsDefaultConnection = "CONNECTIONSTRINGS__DEFAULTCONNECTION";
    public const string DatabaseConnectionString = "DATABASE_CONNECTION_STRING";
    public const string DatabaseUrl = "DATABASE_URL";
    public const string OpenAiApiKey = "OPENAI_API_KEY";
    public const string OpenAiModel = "OPENAI_MODEL";
    public const string OllamaApiKey = "OLLAMA_API_KEY";
    public const string OllamaBaseUrl = "OLLAMA_BASE_URL";
    public const string OllamaModel = "OLLAMA_MODEL";
    public const string JwtSecret = "JWT_SECRET";
    public const string CorsAllowedOrigins = "CORS_ALLOWED_ORIGINS";
}
