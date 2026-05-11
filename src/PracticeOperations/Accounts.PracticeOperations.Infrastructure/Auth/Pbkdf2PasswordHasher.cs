using Accounts.PracticeOperations.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace Accounts.PracticeOperations.Infrastructure.Auth;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _inner = new();
    private static readonly object User = new();

    public string Hash(string plaintextPassword) =>
        _inner.HashPassword(User, plaintextPassword);

    public bool Verify(string hash, string plaintextPassword)
    {
        var result = _inner.VerifyHashedPassword(User, hash, plaintextPassword);
        return result is PasswordVerificationResult.Success
                       or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
