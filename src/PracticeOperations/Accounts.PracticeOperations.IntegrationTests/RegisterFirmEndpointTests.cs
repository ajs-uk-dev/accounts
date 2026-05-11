using System.Net;
using System.Net.Http.Json;
using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class RegisterFirmEndpointTests
{
    private readonly PostgresFixture _pg;
    public RegisterFirmEndpointTests(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task Returns_201_and_ids_when_payload_valid()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();

        var response = await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("Smith & Co", $"smith-{Guid.NewGuid():N}".Substring(0, 12),
                $"owner-{Guid.NewGuid():N}@example.com", "long-enough-password"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<RegisterFirmResult>();
        body!.FirmId.Should().NotBeEmpty();
        body.OwnerUserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Returns_400_when_password_too_short()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var response = await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("X", "x", "a@b.com", "short"));
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
