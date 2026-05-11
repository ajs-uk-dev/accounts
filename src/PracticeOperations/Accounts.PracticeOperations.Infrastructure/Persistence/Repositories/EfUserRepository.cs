using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Repositories;

internal sealed class EfUserRepository : IUserRepository
{
    private readonly PracticeOperationsDbContext _db;
    public EfUserRepository(PracticeOperationsDbContext db) => _db = db;

    public Task<User?> GetAsync(UserId id, CancellationToken ct = default) =>
        _db.Set<User>().IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAcrossFirmsAsync(string email, CancellationToken ct = default)
    {
        var lower = email.Trim().ToLowerInvariant();
        return _db.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email.Value == lower, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await _db.Set<User>().AddAsync(user, ct);
}
