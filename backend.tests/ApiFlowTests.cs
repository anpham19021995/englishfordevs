using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EnglishForDevs.Api.Services;
using EnglishForDevs.Api.Shared;

namespace EnglishForDevs.Api.Tests;

public sealed class ApiFlowTests(ApiTestFactory factory) : IClassFixture<ApiTestFactory>
{
    private readonly HttpClient client = factory.CreateClient();

    [Fact]
    public async Task Register_Then_Login_Returns_Tokens()
    {
        var email = UniqueEmail();

        var register = await RegisterAsync(email);
        var login = await LoginAsync(email);

        Assert.False(string.IsNullOrWhiteSpace(register.Token));
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
        Assert.Equal(email, register.Email);
        Assert.Equal(email, login.Email);
    }

    [Fact]
    public async Task Protected_Endpoints_Reject_Anonymous_Requests()
    {
        var practice = await client.PostAsJsonAsync("/api/practice", new
        {
            mode = PracticeModes.Chat,
            message = "I fixed bug in API."
        });
        var history = await client.GetAsync("/api/practice/history");
        var progress = await client.GetAsync("/api/me/progress");

        Assert.Equal(HttpStatusCode.Unauthorized, practice.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, history.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, progress.StatusCode);
    }

    [Theory]
    [InlineData("not-an-email", "password123", "A valid email is required.")]
    [InlineData("dev@example.com", "short", "Password must be at least 8 characters.")]
    public async Task Register_Rejects_Invalid_Input(
        string email,
        string password,
        string expectedError)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password
        });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedError, error?.Error);
    }

    [Theory]
    [InlineData(PracticeModes.Chat, "", "Message is required.")]
    [InlineData(PracticeModes.Chat, "ok", "Message must be at least 3 characters.")]
    [InlineData("unknown", "I fixed a bug.", "Unsupported mode. Supported modes are: chat, interview, converter.")]
    public async Task Practice_Rejects_Invalid_Input(
        string mode,
        string message,
        string expectedError)
    {
        var auth = await RegisterAsync(UniqueEmail());
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.PostAsJsonAsync("/api/practice", new
        {
            mode,
            message
        });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedError, error?.Error);
    }

    [Fact]
    public async Task Practice_Rejects_Too_Long_Message()
    {
        var auth = await RegisterAsync(UniqueEmail());
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.PostAsJsonAsync("/api/practice", new
        {
            mode = PracticeModes.Chat,
            message = new string('a', ValidationLimits.PracticeMessageMaxLength + 1)
        });
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            $"Message must be {ValidationLimits.PracticeMessageMaxLength} characters or fewer.",
            error?.Error);
    }

    [Fact]
    public async Task Practice_Submit_Saves_User_History()
    {
        var auth = await RegisterAsync(UniqueEmail());
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        var response = await client.PostAsJsonAsync("/api/practice", new
        {
            mode = PracticeModes.Interview,
            message = "I design API with cache and database index."
        });
        var practice = await response.Content.ReadFromJsonAsync<PracticeResponse>();
        var history = await client.GetFromJsonAsync<PracticeHistoryItem[]>(
            "/api/practice/history?take=10");

        response.EnsureSuccessStatusCode();
        Assert.NotNull(practice?.Attempt);
        Assert.Equal(auth.UserId, practice.Attempt?.UserId);
        Assert.NotNull(history);
        Assert.Contains(history, item => item.Id == practice.Attempt?.Id);
    }

    [Fact]
    public async Task Progress_Updates_After_Practice()
    {
        var auth = await RegisterAsync(UniqueEmail());
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", auth.Token);

        await client.PostAsJsonAsync("/api/practice", new
        {
            mode = PracticeModes.Converter,
            message = "Service nay xu ly bat dong bo."
        });
        var progress = await client.GetFromJsonAsync<UserProgressResponse>(
            "/api/me/progress");

        Assert.NotNull(progress);
        Assert.Equal(1, progress.TotalPractices);
        Assert.Equal(1, progress.ConverterPractices);
        Assert.Equal(0, progress.ChatPractices);
        Assert.True(progress.CurrentStreakDays >= 1);
    }

    [Fact]
    public async Task Clear_History_Removes_Only_Current_User_Attempts()
    {
        var firstUserClient = factory.CreateClient();
        var secondUserClient = factory.CreateClient();
        var firstUserAuth = await RegisterAsync(firstUserClient, UniqueEmail());
        var secondUserAuth = await RegisterAsync(secondUserClient, UniqueEmail());
        firstUserClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", firstUserAuth.Token);
        secondUserClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", secondUserAuth.Token);

        await firstUserClient.PostAsJsonAsync("/api/practice", new
        {
            mode = PracticeModes.Chat,
            message = "I fixed the endpoint issue."
        });
        await secondUserClient.PostAsJsonAsync("/api/practice", new
        {
            mode = PracticeModes.Chat,
            message = "I updated the deployment script."
        });

        var clearResponse = await firstUserClient.DeleteAsync("/api/practice/history");
        var firstHistory = await firstUserClient.GetFromJsonAsync<PracticeHistoryItem[]>(
            "/api/practice/history");
        var secondHistory = await secondUserClient.GetFromJsonAsync<PracticeHistoryItem[]>(
            "/api/practice/history");
        var firstProgress = await firstUserClient.GetFromJsonAsync<UserProgressResponse>(
            "/api/me/progress");

        clearResponse.EnsureSuccessStatusCode();
        Assert.Empty(firstHistory ?? []);
        Assert.Single(secondHistory ?? []);
        Assert.NotNull(firstProgress);
        Assert.Equal(0, firstProgress.TotalPractices);
    }

    private async Task<AuthResponse> RegisterAsync(string email)
    {
        return await RegisterAsync(client, email);
    }

    private static async Task<AuthResponse> RegisterAsync(HttpClient httpClient, string email)
    {
        var response = await httpClient.PostAsJsonAsync("/api/auth/register", new
        {
            email,
            password = "password123"
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    private async Task<AuthResponse> LoginAsync(string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "password123"
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    private static string UniqueEmail()
    {
        return $"test-{Guid.NewGuid():N}@example.com";
    }
}
