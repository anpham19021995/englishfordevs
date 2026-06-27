using System.Net.Http.Headers;
using System.Security;
using System.Text;
using System.Text.Json;
using EnglishForDevs.Api.Shared;

namespace EnglishForDevs.Api.Services;

public sealed class AzureSpeechPracticeService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<AzureSpeechPracticeService> logger) : IVoicePracticeService
{
    private const string SpeechLanguage = "en-US";
    private const string TtsOutputFormat = "audio-24khz-48kbitrate-mono-mp3";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string> TranscribeAsync(
        Stream audio,
        string contentType,
        CancellationToken cancellationToken)
    {
        var (key, region) = GetRequiredConfiguration();
        var uri =
            $"https://{region}.stt.speech.microsoft.com/speech/recognition/conversation/cognitiveservices/v1?language={SpeechLanguage}";

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Ocp-Apim-Subscription-Key", key);
        request.Content = new StreamContent(audio);
        request.Content.Headers.TryAddWithoutValidation(
            "Content-Type",
            string.IsNullOrWhiteSpace(contentType)
                ? "audio/wav; codecs=audio/pcm; samplerate=16000"
                : contentType);

        using var response = await SendSpeechRequestAsync(request, "STT", cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "Azure Speech STT returned HTTP {StatusCode}. Body: {ResponseBody}",
                (int)response.StatusCode,
                CleanLog(responseBody));

            throw new InvalidOperationException("Could not transcribe audio.");
        }

        var result = JsonSerializer.Deserialize<AzureSpeechRecognitionResult>(
            responseBody,
            JsonOptions);

        if (result is null ||
            !string.Equals(result.RecognitionStatus, "Success", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(result.DisplayText))
        {
            logger.LogWarning(
                "Azure Speech STT returned status {RecognitionStatus}. Body: {ResponseBody}",
                result?.RecognitionStatus,
                CleanLog(responseBody));

            throw new InvalidOperationException("Speech was unclear. Please try again.");
        }

        return result.DisplayText.Trim();
    }

    public async Task<SpeechAudioResult> SynthesizeAsync(
        string text,
        CancellationToken cancellationToken)
    {
        var (key, region) = GetRequiredConfiguration();
        var voice = configuration[ConfigurationKeys.AzureSpeechVoice] ?? "en-US-JennyNeural";
        var uri = $"https://{region}.tts.speech.microsoft.com/cognitiveservices/v1";
        var ssml =
            $"""
            <speak version="1.0" xml:lang="en-US">
              <voice xml:lang="en-US" xml:gender="Female" name="{SecurityElement.Escape(voice)}">
                {SecurityElement.Escape(text)}
              </voice>
            </speak>
            """;

        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Ocp-Apim-Subscription-Key", key);
        request.Headers.Add("X-Microsoft-OutputFormat", TtsOutputFormat);
        request.Headers.UserAgent.ParseAdd(ApplicationConstants.ApiName);
        request.Content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");

        using var response = await SendSpeechRequestAsync(request, "TTS", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning(
                "Azure Speech TTS returned HTTP {StatusCode}. Body: {ResponseBody}",
                (int)response.StatusCode,
                CleanLog(responseBody));

            throw new InvalidOperationException("Could not create speech audio.");
        }

        var audio = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        return new SpeechAudioResult(audio, "audio/mpeg");
    }

    private (string Key, string Region) GetRequiredConfiguration()
    {
        var key = configuration[ConfigurationKeys.AzureSpeechKey];
        var region = configuration[ConfigurationKeys.AzureSpeechRegion];

        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(region))
        {
            throw new InvalidOperationException("Azure Speech is not configured.");
        }

        return (key, region);
    }

    private async Task<HttpResponseMessage> SendSpeechRequestAsync(
        HttpRequestMessage request,
        string operation,
        CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Azure Speech {Operation} request failed.", operation);
            throw new InvalidOperationException("Could not reach Azure Speech. Please try again.");
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Azure Speech {Operation} request timed out.", operation);
            throw new InvalidOperationException("Azure Speech timed out. Please try again.");
        }
    }

    private static string CleanLog(string value)
    {
        value = value.Trim();
        return value.Length <= 240 ? value : $"{value[..237]}...";
    }

    private sealed record AzureSpeechRecognitionResult(
        string RecognitionStatus,
        string DisplayText);
}
