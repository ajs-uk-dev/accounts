using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using Accounts.SharedKernel.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class FirmPersistenceTests
{
    private readonly PostgresFixture _pg;
    public FirmPersistenceTests(PostgresFixture pg) => _pg = pg;

    private sealed class FakeFirmContext : IFirmContext
    {
        public FirmId? FirmId { get; set; }
        public UserId? UserId { get; set; }
        public bool IsAuthenticated => FirmId.HasValue;
    }

    [Fact]
    public async Task Round_trips_firm_through_db()
    {
        // Firm is NOT tenant-scoped, so no FirmId required on the context.
        var ctx = new FakeFirmContext();
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        var now = new DateTimeOffset(2026, 5, 11, 12, 0, 0, TimeSpan.Zero);
        var slug = $"roundtrip-{Guid.NewGuid():N}".Substring(0, 24);
        var firm = Firm.Register("Acme Bookkeeping LLP", slug, now);
        var firmId = firm.Id;

        using (var scope = api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Set<Firm>().Add(firm);
            await db.SaveChangesAsync();
        }

        using (var scope = api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            var loaded = await db.Set<Firm>().SingleAsync(f => f.Id == firmId);

            loaded.Id.Should().Be(firmId);
            loaded.Name.Should().Be("Acme Bookkeeping LLP");
            loaded.Slug.Should().Be(slug);
            loaded.Status.Should().Be(FirmStatus.Trial);
            loaded.CreatedAt.Should().Be(now);
            loaded.UpdatedAt.Should().Be(now);
        }
    }

    [Fact]
    public async Task Slug_unique_constraint_is_enforced()
    {
        var ctx = new FakeFirmContext();
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        var now = DateTimeOffset.UtcNow;
        var sharedSlug = $"dupe-{Guid.NewGuid():N}".Substring(0, 24);
        var first = Firm.Register("First Firm", sharedSlug, now);
        var second = Firm.Register("Second Firm", sharedSlug, now);

        using (var scope = api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Set<Firm>().Add(first);
            await db.SaveChangesAsync();
        }

        using (var scope = api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Set<Firm>().Add(second);
            var act = () => db.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateException>();
        }
    }
}
