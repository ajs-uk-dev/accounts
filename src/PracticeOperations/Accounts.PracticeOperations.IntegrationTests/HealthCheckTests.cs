using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class HealthCheckTests
{
    private readonly PostgresFixture _pg;
    public HealthCheckTests(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task Health_endpoint_returns_healthy_when_database_reachable()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();

        var response = await client.GetAsync("/health");

        response.IsSuccessStatusCode.Should().BeTrue();
        (await response.Content.ReadAsStringAsync()).Should().Be("Healthy");
    }
}
