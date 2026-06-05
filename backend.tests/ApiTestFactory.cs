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
                ["ConnectionStrings:DefaultConnection"] = "",
                ["Jwt:Secret"] = "development-only-secret-change-me-please-32-chars",
                ["Jwt:Issuer"] = "EnglishForDevs",
                ["Jwt:Audience"] = "EnglishForDevs",
                ["OpenAI:ApiKey"] = "",
                ["OpenAI:Model"] = "gpt-4o-mini"
            });
        });
    }
}
