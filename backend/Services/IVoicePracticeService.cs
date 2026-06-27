namespace EnglishForDevs.Api.Services;

public interface IVoicePracticeService
{
    Task<string> TranscribeAsync(
        Stream audio,
        string contentType,
        CancellationToken cancellationToken);

    Task<SpeechAudioResult> SynthesizeAsync(
        string text,
        CancellationToken cancellationToken);
}

public sealed record SpeechAudioResult(byte[] Audio, string ContentType);
