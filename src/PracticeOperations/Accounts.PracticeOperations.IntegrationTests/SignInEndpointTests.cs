using System.Net;
using System.Net.Http.Json;
using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.PracticeOperations.Application.Users.SignIn;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using FluentAssertions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class SignInEndpointTests
{
    private readonly PostgresFixture _pg;
    public SignInEndpointTests(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task SignIn_returns_access_token_after_registration_without_totp()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var slug = $"signin-{Guid.NewGuid():N}".Substring(0, 12);
        var email = $"u-{Guid.NewGuid():N}@example.com";
        var pwd = "long-enough-password";

        var register = await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("X", slug, email, pwd));
        register.IsSuccessStatusCode.Should().BeTrue();

        var signIn = await client.PostAsJsonAsync("/api/auth/sign-in",
            new SignInCommand(email, pwd, TotpCode: null));
        signIn.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await signIn.Content.ReadFromJsonAsync<SignInResult>();
        body!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SignIn_returns_401_for_wrong_password()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var email = $"u-{Guid.NewGuid():N}@example.com";
        await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("X", $"x-{Guid.NewGuid():N}".Substring(0, 12), email, "correct-password-1"));
        var bad = await client.PostAsJsonAsync("/api/auth/sign-in",
            new SignInCommand(email, "wrong-password-1", null));
        bad.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
