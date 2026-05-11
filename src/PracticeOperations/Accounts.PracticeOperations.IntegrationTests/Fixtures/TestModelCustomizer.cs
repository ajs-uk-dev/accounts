using System.Linq.Expressions;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Accounts.PracticeOperations.IntegrationTests.Fixtures;

/// <summary>
/// Registers test-only entity configurations (e.g. <see cref="TenantTestRow"/>) into the
/// EF model at startup, so the production <c>PracticeOperationsDbContext</c> does not need
/// to know about test entities.  Registered via
/// <c>ReplaceService&lt;IModelCustomizer, TestModelCustomizer&gt;()</c> in <see cref="ApiFactory"/>.
/// </summary>
internal sealed class TestModelCustomizer : RelationalModelCustomizer
{
    public TestModelCustomizer(ModelCustomizerDependencies dependencies)
        : base(dependencies) { }

    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);

        // Add TenantTestRow to the model.
        modelBuilder.ApplyConfiguration(new TenantTestRowConfiguration());

        // Apply the same tenant query filter that PracticeOperationsDbContext applies to
        // ITenantScopedEntity types.  The filter loop in OnModelCreating runs before this
        // method returns, so we must apply the filter here explicitly.
        if (context is PracticeOperationsDbContext practiceDb)
        {
            Expression<Func<TenantTestRow, bool>> filter =
                e => practiceDb.CurrentFirmId == null || e.FirmId == practiceDb.CurrentFirmId.Value;
            modelBuilder.Entity<TenantTestRow>().HasQueryFilter(filter);
        }
    }
}
