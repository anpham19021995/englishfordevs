using EnglishForDevs.Api.Services;
using EnglishForDevs.Api.Shared;

namespace EnglishForDevs.Api.Features.Practice;

public static class TranscribePracticeAudio
{
    public static async Task<IResult> HandleAsync(
        IFormFile audio,
        IVoicePracticeService voiceService,
        CancellationToken cancellationToken)
    {
        if (audio.Length <= 0)
        {
            return Results.BadRequest(new { error = "Audio file is required." });
        }

        if (audio.Length > ValidationLimits.VoiceAudioMaxBytes)
        {
            return Results.BadRequest(new { error = "Audio file must be 10 MB or smaller." });
        }

        if (!audio.ContentType.Contains("wav", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { error = "Audio must be a WAV recording." });
        }

        try
        {
            await using var stream = audio.OpenReadStream();
            var transcript = await voiceService.TranscribeAsync(
                stream,
                "audio/wav; codecs=audio/pcm; samplerate=16000",
                cancellationToken);

            return Results.Ok(new TranscribePracticeAudioResponse(transcript));
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    }
}
