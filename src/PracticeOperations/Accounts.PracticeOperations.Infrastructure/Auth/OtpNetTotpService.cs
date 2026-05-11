using Accounts.PracticeOperations.Application.Abstractions;
using OtpNet;

namespace Accounts.PracticeOperations.Infrastructure.Auth;

public sealed class OtpNetTotpService : ITotpService
{
    public string GenerateSecret()
    {
        var bytes = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(bytes);
    }

    public string BuildOtpAuthUri(string secret, string accountName, string issuer)
    {
        var encIssuer = Uri.EscapeDataString(issuer);
        var encAccount = Uri.EscapeDataString(accountName);
        return $"otpauth://totp/{encIssuer}:{encAccount}?secret={secret}&issuer={encIssuer}&digits=6&period=30";
    }

    public bool Verify(string secret, string code)
    {
        var bytes = Base32Encoding.ToBytes(secret);
        var totp = new Totp(bytes);
        return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
    }
}
