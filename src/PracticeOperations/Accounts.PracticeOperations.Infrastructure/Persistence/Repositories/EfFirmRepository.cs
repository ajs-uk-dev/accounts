using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Repositories;

internal sealed class EfFirmRepository : IFirmRepository
{
    private readonly PracticeOperationsDbContext _db;
    public EfFirmRepository(PracticeOperationsDbContext db) => _db = db;

    public Task<Firm?> GetAsync(FirmId id, CancellationToken ct = default) =>
        _db.Set<Firm>().IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Id == id, ct);

    public Task<Firm?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        _db.Set<Firm>().IgnoreQueryFilters().FirstOrDefaultAsync(f => f.Slug == slug, ct);

    public async Task AddAsync(Firm firm, CancellationToken ct = default) =>
        await _db.Set<Firm>().AddAsync(firm, ct);
}
