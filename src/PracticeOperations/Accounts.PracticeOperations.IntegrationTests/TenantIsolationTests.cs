using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain._Test;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using Accounts.SharedKernel.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class TenantIsolationTests
{
    private static readonly string[] ExpectedAlpha = ["alpha-A"];
    private static readonly string[] ExpectedBeta = ["beta-B"];

    private readonly PostgresFixture _pg;
    public TenantIsolationTests(PostgresFixture pg) => _pg = pg;

    private sealed class FakeFirmContext : IFirmContext
    {
        public FirmId? FirmId { get; set; }
        public UserId? UserId { get; set; }
        public bool IsAuthenticated => FirmId.HasValue;
    }

    [Fact]
    public async Task Query_filter_hides_rows_from_other_tenants()
    {
        var firmA = SharedKernel.Identity.FirmId.New();
        var firmB = SharedKernel.Identity.FirmId.New();
        var ctx = new FakeFirmContext();

        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        // seed both firms (use raw DbContext to bypass filter)
        using (var scope = api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            ctx.FirmId = null;        // bypass — null FirmId = no filter
            db.Set<TenantTestRow>().AddRange(
                new TenantTestRow(firmA, "alpha-A"),
                new TenantTestRow(firmB, "beta-B"));
            await db.SaveChangesAsync();
        }

        // scope to firmA — should see only alpha-A
        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = firmA;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            var rows = await db.Set<TenantTestRow>().Select(r => r.Label).ToListAsync();
            rows.Should().BeEquivalentTo(ExpectedAlpha);
        }

        // scope to firmB — should see only beta-B
        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = firmB;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            var rows = await db.Set<TenantTestRow>().Select(r => r.Label).ToListAsync();
            rows.Should().BeEquivalentTo(ExpectedBeta);
        }
    }
}
