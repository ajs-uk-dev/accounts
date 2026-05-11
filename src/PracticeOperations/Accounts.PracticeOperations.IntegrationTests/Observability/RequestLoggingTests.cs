using System.Net.Http.Headers;
using System.Net.Http.Json;
using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.PracticeOperations.Application.Users.SignIn;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Accounts.PracticeOperations.IntegrationTests.Observability;

/// <summary>
/// Minimal in-memory Serilog sink for capturing log events in tests.
/// </summary>
internal sealed class InMemorySink : ILogEventSink
{
    private readonly List<LogEvent> _events = new();
    private readonly object _lock = new();

    public IReadOnlyList<LogEvent> Events
    {
        get { lock (_lock) { return _events.ToList(); } }
    }

    public void Emit(LogEvent logEvent)
    {
        lock (_lock) { _events.Add(logEvent); }
    }
}

/// <summary>
/// Verifies that the Serilog request-completion log event (emitted by
/// UseSerilogRequestLogging) carries FirmId, UserId and CorrelationId
/// properties when the middleware pipeline is ordered correctly.
/// </summary>
[Collection(nameof(PostgresCollection))]
public class RequestLoggingTests
{
    private readonly PostgresFixture _pg;
    public RequestLoggingTests(PostgresFixture pg) => _pg = pg;

    private sealed class RequestLoggingFactory : WebApplicationFactory<Program>
    {
        private readonly string _connectionString;
        private readonly InMemorySink _sink;

        public RequestLoggingFactory(string connectionString, InMemorySink sink)
        {
            _connectionString = connectionString;
            _sink = sink;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:PracticeOperations", _connectionString);
            builder.UseSetting("Jwt:Issuer", "accounts-test");
            builder.UseSetting("Jwt:Audience", "accounts-test");
            builder.UseSetting("Jwt:Secret", "TEST-SECRET-do-not-use-in-prod-min-32-chars-xx");
            builder.UseSetting("Jwt:LifetimeMinutes", "60");

            builder.ConfigureServices(services =>
            {
                using var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
                db.Database.Migrate();
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Replace the Serilog pipeline with one that writes to our in-memory sink.
            // Must be done on IHostBuilder (not IWebHostBuilder) to override UseSerilog from Program.cs.
            builder.UseSerilog((_, lc) =>
                lc.Enrich.FromLogContext()
                  .WriteTo.Sink(_sink));
            return base.CreateHost(builder);
        }
    }

    [Fact]
    public async Task Authenticated_request_log_carries_FirmId_UserId_and_CorrelationId()
    {
        var sink = new InMemorySink();
        await using var api = new RequestLoggingFactory(_pg.ConnectionString, sink);
        var client = api.CreateClient();

        // Register a firm and sign in to get a token.
        var slug = $"rl-{Guid.NewGuid():N}"[..12];
        var email = $"rl-{Guid.NewGuid():N}@example.com";
        const string pwd = "long-enough-password";

        await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("RL Firm", slug, email, pwd));

        var signInResp = await client.PostAsJsonAsync("/api/auth/sign-in",
            new SignInCommand(email, pwd, null));
        var token = (await signInResp.Content.ReadFromJsonAsync<SignInResult>())!.AccessToken;

        // Hit an authenticated endpoint.
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await client.GetAsync("/api/admin/me");

        // Allow a brief moment for async log emission to complete (usually not needed, but defensive).
        await Task.Delay(50);

        // Find the Serilog request-completion event (has RequestMethod property).
        var requestEvent = sink.Events
            .Where(e => e.Properties.ContainsKey("RequestMethod"))
            .LastOrDefault();

        requestEvent.Should().NotBeNull("UseSerilogRequestLogging should emit a request-completion event");
        requestEvent!.Properties.Should().ContainKey("CorrelationId",
            "correlation middleware must run before Serilog request logging");
        requestEvent.Properties["CorrelationId"].ToString().Trim('"').Should().NotBeNullOrEmpty();

        requestEvent.Properties.Should().ContainKey("FirmId",
            "TenantLogContext must be captured by the Serilog request-completion log");
        var firmIdStr = requestEvent.Properties["FirmId"].ToString().Trim('"');
        firmIdStr.Should().NotBe(Guid.Empty.ToString(),
            "FirmId should be the authenticated firm, not the default empty guid");

        requestEvent.Properties.Should().ContainKey("UserId",
            "TenantLogContext must push UserId so the request-completion log captures it");
        var userIdStr = requestEvent.Properties["UserId"].ToString().Trim('"');
        userIdStr.Should().NotBe(Guid.Empty.ToString(),
            "UserId should be the authenticated user, not the default empty guid");
    }

    [Fact]
    public async Task Anonymous_request_log_carries_CorrelationId()
    {
        var sink = new InMemorySink();
        await using var api = new RequestLoggingFactory(_pg.ConnectionString, sink);
        var client = api.CreateClient();

        await client.GetAsync("/health");

        await Task.Delay(50);

        var requestEvent = sink.Events
            .Where(e => e.Properties.ContainsKey("RequestMethod"))
            .LastOrDefault();

        requestEvent.Should().NotBeNull("UseSerilogRequestLogging should emit a request-completion event for /health");
        requestEvent!.Properties.Should().ContainKey("CorrelationId",
            "correlation middleware must run before Serilog request logging");
        requestEvent.Properties["CorrelationId"].ToString().Trim('"').Should().NotBeNullOrEmpty();
    }
}
