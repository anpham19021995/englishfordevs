namespace EnglishForDevs.Api.Shared;

public static class ApplicationConstants
{
    public const string Name = "EnglishForDevs";
    public const string ApiName = "EnglishForDevs.Api";
    public const string FrontendCorsPolicy = "Frontend";
    public const string DevelopmentJwtSecret = "development-only-secret-change-me-please-32-chars";
}

public static class AiProviders
{
    public const string OpenAi = "openai";
    public const string Ollama = "ollama";
}

public static class PracticeSources
{
    public const string OpenAi = AiProviders.OpenAi;
    public const string Ollama = AiProviders.Ollama;
    public const string LocalFallback = "local-fallback";
}

public static class HistoryStorageTypes
{
    public const string InMemory = "in-memory";
    public const string Postgres = "postgres";
}

public static class ValidationLimits
{
    public const int EmailMaxLength = 256;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;
    public const int PracticeMessageMinLength = 3;
    public const int PracticeMessageMaxLength = 4000;
    public const int PracticeFeedbackMaxLength = 4000;
    public const int PracticeFollowUpQuestionMaxLength = 1000;
}
