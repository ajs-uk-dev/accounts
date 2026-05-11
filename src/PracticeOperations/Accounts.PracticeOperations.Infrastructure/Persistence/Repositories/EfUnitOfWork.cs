using Accounts.PracticeOperations.Application.Abstractions;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Repositories;

internal sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly PracticeOperationsDbContext _db;
    public EfUnitOfWork(PracticeOperationsDbContext db) => _db = db;
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
