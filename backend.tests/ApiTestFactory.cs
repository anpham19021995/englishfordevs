using EnglishForDevs.Api.Shared;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace EnglishForDevs.Api.Tests;

public sealed class ApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                [ConfigurationKeys.DefaultConnectionPath] = "",
                [ConfigurationKeys.JwtSecret] = ApplicationConstants.DevelopmentJwtSecret,
                [ConfigurationKeys.JwtIssuer] = ApplicationConstants.Name,
                [ConfigurationKeys.JwtAudience] = ApplicationConstants.Name,
                [ConfigurationKeys.OpenAiApiKey] = "",
                [ConfigurationKeys.OpenAiModel] = "gpt-4o-mini"
            });
        });
    }
}
