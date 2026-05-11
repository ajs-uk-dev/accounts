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
        // EF cannot translate `u.Email.Value == lower` because Email is mapped via a value
        // converter (EmailAddress <-> string column). Compare against a constructed EmailAddress
        // instance instead — EF's value-converter equality applies the converter and emits a
        // plain `where email = @p0`. If the input fails the regex it can't match any stored
        // value, so short-circuit.
        var target = EmailAddress.Create(email);
        if (target.IsFailure) return Task.FromResult<User?>(null);
        var emailValue = target.Value!;
        return _db.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == emailValue, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await _db.Set<User>().AddAsync(user, ct);
}
