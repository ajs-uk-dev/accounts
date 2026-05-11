using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetAsync(UserId id, CancellationToken ct = default);
    /// <summary>Look up by email within a firm. Bypasses tenant filter (used by SignIn before context is set).</summary>
    Task<User?> GetByEmailAcrossFirmsAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}
