using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Amnecyne.LinkShort.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"linkshort-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}",
                ["Jwt:Key"] = "ThisIsALongerSecureJwtKeyForTests_12345",
                ["Jwt:Issuer"] = "AmnecyneTests",
                ["Jwt:Audience"] = "AmnecyneTests"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_dbPath))
            try
            {
                File.Delete(_dbPath);
            }
            catch (IOException)
            {
            }
    }
}