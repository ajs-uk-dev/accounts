namespace Accounts.PracticeOperations.Application.Abstractions;

public interface ITotpService
{
    /// <summary>Generate a new Base32 secret for enrolment.</summary>
    string GenerateSecret();
    /// <summary>otpauth:// URI that authenticator apps consume as a QR code.</summary>
    string BuildOtpAuthUri(string secret, string accountName, string issuer);
    /// <summary>Verify a 6-digit code against the secret with ±1 step tolerance.</summary>
    bool Verify(string secret, string code);
}
