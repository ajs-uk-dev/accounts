using System.Linq.Expressions;
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;

namespace Accounts.PracticeOperations.Infrastructure.Persistence;

public class PracticeOperationsDbContext : DbContext
{
    private readonly IFirmContext _firmContext;

    public PracticeOperationsDbContext(
        DbContextOptions<PracticeOperationsDbContext> options,
        IFirmContext firmContext)
        : base(options)
    {
        _firmContext = firmContext;
    }

    /// <summary>Strongly-typed accessor used by the query filter so EF can apply the FirmId value converter symmetrically.</summary>
    internal FirmId? CurrentFirmId => _firmContext.FirmId;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("practice_operations");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PracticeOperationsDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantScopedEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(PracticeOperationsDbContext)
                    .GetMethod(nameof(SetTenantFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private void SetTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : class, ITenantScopedEntity
    {
        // Compare the strongly-typed FirmId on both sides so EF applies the value
        // converter on the LHS column and on the captured parameter symmetrically.
        Expression<Func<TEntity, bool>> filter =
            e => CurrentFirmId == null || e.FirmId == CurrentFirmId.Value;
        modelBuilder.Entity<TEntity>().HasQueryFilter(filter);
    }

    public override int SaveChanges()
    {
        GuardAuditAppendOnly();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        GuardAuditAppendOnly();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void GuardAuditAppendOnly()
    {
        foreach (var entry in ChangeTracker.Entries<Accounts.PracticeOperations.Domain.Audit.AuditEvent>())
        {
            if (entry.State is EntityState.Modified or EntityState.Deleted)
                throw new InvalidOperationException(
                    "AuditEvent is append-only; updates and deletes are not permitted.");
        }
    }
}
