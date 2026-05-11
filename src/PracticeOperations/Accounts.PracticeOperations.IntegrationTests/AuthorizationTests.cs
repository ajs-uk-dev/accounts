using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.PracticeOperations.Application.Users.SignIn;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class AuthorizationTests
{
    private readonly PostgresFixture _pg;
    public AuthorizationTests(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task Anonymous_request_to_admin_returns_401()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var response = await client.GetAsync("/api/admin/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FirmOwner_can_access_owner_only_endpoint()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var slug = $"a-{Guid.NewGuid():N}".Substring(0, 12);
        var email = $"u-{Guid.NewGuid():N}@example.com";
        var pwd = "long-enough-password";
        await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("X", slug, email, pwd));
        var signIn = await client.PostAsJsonAsync("/api/auth/sign-in",
            new SignInCommand(email, pwd, null));
        var token = (await signIn.Content.ReadFromJsonAsync<SignInResult>())!.AccessToken;

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/api/admin/owner-only");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
