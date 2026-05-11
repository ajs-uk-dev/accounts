using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.PracticeOperations.Infrastructure.Auth;
using Accounts.SharedKernel.Identity;
using Accounts.SharedKernel.Time;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;

namespace Accounts.PracticeOperations.UnitTests.Application;

public class JwtIssuerTests
{
    private const string TestSecret = "TEST-SECRET-do-not-use-in-prod-min-32-chars-xx";
    private static IConfiguration BuildConfig() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:Secret"] = TestSecret,
            ["Jwt:LifetimeMinutes"] = "30",
        })
        .Build();

    [Fact]
    public void Issue_produces_token_with_required_claims_and_expiry()
    {
        var clock = Substitute.For<IClock>();
        var now = new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero);
        clock.UtcNow.Returns(now);

        var issuer = new JwtIssuer(BuildConfig(), clock);
        var firmId = new FirmId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());

        var (token, expiresAt) = issuer.Issue(firmId, userId, Role.FirmOwner);

        token.Should().NotBeNullOrWhiteSpace();
        expiresAt.Should().Be(now.AddMinutes(30));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Issuer.Should().Be("test-issuer");
        jwt.Audiences.Should().ContainSingle().Which.Should().Be("test-audience");
        jwt.Claims.Should().Contain(c => c.Type == "sub" && c.Value == userId.Value.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "firm_id" && c.Value == firmId.Value.ToString());
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "FirmOwner");
    }

    [Fact]
    public void Issued_token_validates_against_the_same_signing_key()
    {
        var clock = Substitute.For<IClock>();
        clock.UtcNow.Returns(new DateTimeOffset(2026, 5, 11, 10, 0, 0, TimeSpan.Zero));

        var issuer = new JwtIssuer(BuildConfig(), clock);
        var (token, _) = issuer.Issue(
            new FirmId(Guid.NewGuid()), new UserId(Guid.NewGuid()), Role.Manager);

        var handler = new JwtSecurityTokenHandler();
        var act = () => handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidIssuer = "test-issuer",
            ValidAudience = "test-audience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false,  // Frozen clock vs validator's real-time
        }, out _);

        act.Should().NotThrow();
    }

    [Fact]
    public void Throws_when_secret_config_missing()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?> { ["Jwt:Issuer"] = "x", ["Jwt:Audience"] = "y" }).Build();

        var act = () => new JwtIssuer(config, Substitute.For<IClock>());

        act.Should().Throw<InvalidOperationException>().WithMessage("*Jwt:Secret*");
    }
}
