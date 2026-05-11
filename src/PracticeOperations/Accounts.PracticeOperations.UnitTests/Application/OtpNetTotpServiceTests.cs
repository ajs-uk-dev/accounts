using Accounts.PracticeOperations.Infrastructure.Auth;
using FluentAssertions;
using OtpNet;

namespace Accounts.PracticeOperations.UnitTests.Application;

public class OtpNetTotpServiceTests
{
    [Fact]
    public void GenerateSecret_returns_valid_base32_of_20_bytes()
    {
        var svc = new OtpNetTotpService();
        var secret = svc.GenerateSecret();

        secret.Should().NotBeNullOrWhiteSpace();
        // 20 raw bytes -> 32 base32 chars
        secret.Length.Should().Be(32);
        // Round-trip-able through OtpNet's Base32 decoder
        var act = () => Base32Encoding.ToBytes(secret);
        act.Should().NotThrow();
    }

    [Fact]
    public void BuildOtpAuthUri_encodes_issuer_and_account_and_includes_required_params()
    {
        var svc = new OtpNetTotpService();
        var uri = svc.BuildOtpAuthUri("JBSWY3DPEHPK3PXP", "alice@example.com", "Accounts SaaS");

        uri.Should().StartWith("otpauth://totp/Accounts%20SaaS:alice%40example.com?");
        uri.Should().Contain("secret=JBSWY3DPEHPK3PXP");
        uri.Should().Contain("issuer=Accounts%20SaaS");
        uri.Should().Contain("digits=6");
        uri.Should().Contain("period=30");
    }

    [Fact]
    public void Verify_accepts_a_freshly_generated_code()
    {
        var svc = new OtpNetTotpService();
        var secret = svc.GenerateSecret();
        // Generate a current code the way an authenticator app would.
        var totp = new Totp(Base32Encoding.ToBytes(secret));
        var code = totp.ComputeTotp();

        svc.Verify(secret, code).Should().BeTrue();
        svc.Verify(secret, "000000").Should().BeFalse();
    }
}
