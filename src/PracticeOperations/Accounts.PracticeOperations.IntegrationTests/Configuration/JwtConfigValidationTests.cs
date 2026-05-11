using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.PracticeOperations.IntegrationTests.Configuration;

/// <summary>
/// Verifies that the application fails fast at startup when JWT configuration is
/// missing, empty, or the secret is too short for HMAC-SHA256 (requires ≥ 32 bytes).
/// </summary>
[Collection(nameof(PostgresCollection))]
public class JwtConfigValidationTests
{
    private readonly PostgresFixture _pg;
    public JwtConfigValidationTests(PostgresFixture pg) => _pg = pg;

    /// <summary>
    /// Factory that starts with a valid baseline (copies ApiFactory behaviour) but
    /// allows individual settings to be overridden or cleared for each test case.
    /// </summary>
    private sealed class JwtTestFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, string?> _overrides;

        public JwtTestFactory(string connectionString, Dictionary<string, string?> overrides)
        {
            _connectionString = connectionString;
            _overrides = overrides;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:PracticeOperations", _connectionString);
            // Start from a valid baseline so only the overridden key is bad.
            builder.UseSetting("Jwt:Issuer", "accounts-test");
            builder.UseSetting("Jwt:Audience", "accounts-test");
            builder.UseSetting("Jwt:Secret", "TEST-SECRET-do-not-use-in-prod-min-32-chars-xx");
            builder.UseSetting("Jwt:LifetimeMinutes", "60");

            // Apply test-specific overrides (null value = clear/delete the setting).
            foreach (var (key, value) in _overrides)
            {
                if (value is null)
                    builder.UseSetting(key, string.Empty);
                else
                    builder.UseSetting(key, value);
            }

            builder.ConfigureServices(services =>
            {
                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
                db.Database.Migrate();
            });
        }
    }

    [Fact]
    public void CreateClient_throws_when_Jwt_Secret_is_empty()
    {
        using var factory = new JwtTestFactory(_pg.ConnectionString,
            new Dictionary<string, string?> { ["Jwt:Secret"] = "" });

        var act = () => factory.CreateClient();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Secret*");
    }

    [Fact]
    public void CreateClient_throws_when_Jwt_Secret_is_shorter_than_32_bytes()
    {
        // 10-character secret — too short for HMAC-SHA256
        using var factory = new JwtTestFactory(_pg.ConnectionString,
            new Dictionary<string, string?> { ["Jwt:Secret"] = "tooshort10" });

        var act = () => factory.CreateClient();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Secret*");
    }

    [Fact]
    public void CreateClient_throws_when_Jwt_Issuer_is_empty()
    {
        using var factory = new JwtTestFactory(_pg.ConnectionString,
            new Dictionary<string, string?> { ["Jwt:Issuer"] = "" });

        var act = () => factory.CreateClient();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Issuer*");
    }

    [Fact]
    public void CreateClient_throws_when_Jwt_Audience_is_empty()
    {
        using var factory = new JwtTestFactory(_pg.ConnectionString,
            new Dictionary<string, string?> { ["Jwt:Audience"] = "" });

        var act = () => factory.CreateClient();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt:Audience*");
    }
}
