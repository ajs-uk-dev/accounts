using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using Accounts.Web.Middleware;
using FluentAssertions;

namespace Accounts.PracticeOperations.IntegrationTests.Observability;

[Collection(nameof(PostgresCollection))]
public class CorrelationIdTests
{
    private readonly PostgresFixture _pg;
    public CorrelationIdTests(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task Request_with_correlation_id_header_echoes_same_value_in_response()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        const string knownId = "abc123def456";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.TryAddWithoutValidation(CorrelationIdMiddleware.HeaderName, knownId);

        var response = await client.SendAsync(request);

        response.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out var values)
            .Should().BeTrue("response should include the correlation-id header");
        values!.Should().ContainSingle(v => v == knownId,
            "the same correlation-id sent in the request should be echoed back");
    }

    [Fact]
    public async Task Request_without_correlation_id_header_receives_generated_32char_hex_value()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();

        var response = await client.GetAsync("/health");

        response.Headers.TryGetValues(CorrelationIdMiddleware.HeaderName, out var values)
            .Should().BeTrue("response should always include a correlation-id header");
        var id = values!.Should().ContainSingle().Subject;
        id.Should().HaveLength(32, "Guid.ToString(\"N\") produces a 32-character lowercase hex string");
        id.Should().MatchRegex("^[0-9a-f]{32}$", "value should be lowercase hex digits only");
    }
}
