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

    /// <summary>For migration-only / test-fixture use where no firm context exists.</summary>
    internal Guid? CurrentFirmIdRaw =>
        _firmContext.FirmId.HasValue ? _firmContext.FirmId.Value.Value : null;

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
        Expression<Func<TEntity, bool>> filter =
            e => CurrentFirmIdRaw == null || e.FirmId.Value == CurrentFirmIdRaw;
        modelBuilder.Entity<TEntity>().HasQueryFilter(filter);
    }
}
