using EnglishForDevs.Api.Services;
using EnglishForDevs.Api.Shared;

namespace EnglishForDevs.Api.Features.Practice;

public static class PracticeValidation
{
    public static string? Validate(PracticeRequest request)
    {
        var message = request.Message.Trim();

        if (string.IsNullOrWhiteSpace(message))
        {
            return "Message is required.";
        }

        if (message.Length < ValidationLimits.PracticeMessageMinLength)
        {
            return $"Message must be at least {ValidationLimits.PracticeMessageMinLength} characters.";
        }

        if (message.Length > ValidationLimits.PracticeMessageMaxLength)
        {
            return $"Message must be {ValidationLimits.PracticeMessageMaxLength} characters or fewer.";
        }

        if (!PracticeModes.All.Contains(request.Mode, StringComparer.OrdinalIgnoreCase))
        {
            return $"Unsupported mode. Supported modes are: {string.Join(", ", PracticeModes.All)}.";
        }

        return null;
    }

    public static PracticeRequest Normalize(PracticeRequest request)
    {
        return request with
        {
            Mode = request.Mode.Trim().ToLowerInvariant(),
            Message = request.Message.Trim()
        };
    }
}
