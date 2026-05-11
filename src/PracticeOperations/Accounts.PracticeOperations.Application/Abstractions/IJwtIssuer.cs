using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IJwtIssuer
{
    /// <summary>Returns (accessToken, expiresAt).</summary>
    (string Token, DateTimeOffset ExpiresAt) Issue(FirmId firmId, UserId userId, Role role);
}
