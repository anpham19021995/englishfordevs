using EnglishForDevs.Api.Services;
using EnglishForDevs.Api.Shared;

namespace EnglishForDevs.Api.Features.Practice;

public static class SynthesizePracticeSpeech
{
    public static async Task<IResult> HandleAsync(
        TextToSpeechRequest request,
        IVoicePracticeService voiceService,
        CancellationToken cancellationToken)
    {
        var text = request.Text.Trim();

        if (string.IsNullOrWhiteSpace(text))
        {
            return Results.BadRequest(new { error = "Text is required." });
        }

        if (text.Length > ValidationLimits.TextToSpeechMaxLength)
        {
            return Results.BadRequest(new { error = "Text must be 1000 characters or fewer." });
        }

        try
        {
            var result = await voiceService.SynthesizeAsync(text, cancellationToken);
            return Results.File(result.Audio, result.ContentType);
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    }
}
