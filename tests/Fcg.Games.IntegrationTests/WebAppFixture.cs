using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Fcg.Games.IntegrationTests;

public class WebAppFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var authority = TestOidcServer.BaseUrl;
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UseInMemoryDatabase"] = "true",
                ["Jwt:Authority"] = authority,
                ["Jwt:Audience"] = "fcg-cloud-platform",
                ["Jwt:RequireHttpsMetadata"] = "false",
                ["InternalApi:ApiKey"] = "test-internal-api-key"
            });
        });
    }
}
