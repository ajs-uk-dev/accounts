using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Audit;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using Accounts.SharedKernel.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class AuditLogTests
{
    private readonly PostgresFixture _pg;
    public AuditLogTests(PostgresFixture pg) => _pg = pg;

    private sealed class FakeFirmContext : IFirmContext
    {
        public FirmId? FirmId { get; set; }
        public UserId? UserId { get; set; }
        public bool IsAuthenticated => FirmId.HasValue;
    }

    [Fact]
    public async Task RecordAsync_persists_event_for_current_firm()
    {
        var firm = FirmId.New();
        var ctx = new FakeFirmContext { FirmId = firm, UserId = UserId.New() };
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        using (var scope = api.Services.CreateScope())
        {
            var writer = scope.ServiceProvider.GetRequiredService<IAuditWriter>();
            await writer.RecordAsync(AuditAction.UserSignedIn, "User", ctx.UserId!.Value.ToString());
        }

        using (var scope = api.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            var rows = await db.Set<AuditEvent>().ToListAsync();
            rows.Should().HaveCount(1);
            rows[0].FirmId.Should().Be(firm);
            rows[0].Action.Should().Be(AuditAction.UserSignedIn);
        }
    }

    [Fact]
    public async Task SaveChanges_throws_when_AuditEvent_is_modified()
    {
        var firm = FirmId.New();
        var ctx = new FakeFirmContext { FirmId = firm, UserId = UserId.New() };
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
        var evt = AuditEvent.Record(firm, ctx.UserId, AuditAction.UserSignedIn,
            "User", ctx.UserId!.Value.ToString(), null, null, DateTimeOffset.UtcNow);
        db.Set<AuditEvent>().Add(evt);
        await db.SaveChangesAsync();

        // Tamper via change tracker (private setters can't be hit directly)
        db.Entry(evt).Property(nameof(AuditEvent.EntityId)).CurrentValue = "tampered";

        var act = () => db.SaveChangesAsync();
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }

    [Fact]
    public async Task SaveChangesAsync_two_arg_overload_also_blocks_modification()
    {
        var firm = FirmId.New();
        var ctx = new FakeFirmContext { FirmId = firm, UserId = UserId.New() };
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
        var evt = AuditEvent.Record(firm, ctx.UserId, AuditAction.UserSignedIn,
            "User", ctx.UserId!.Value.ToString(), null, null, DateTimeOffset.UtcNow);
        db.Set<AuditEvent>().Add(evt);
        await db.SaveChangesAsync(acceptAllChangesOnSuccess: true, CancellationToken.None);

        db.Entry(evt).Property(nameof(AuditEvent.EntityId)).CurrentValue = "tampered";

        var act = () => db.SaveChangesAsync(acceptAllChangesOnSuccess: true, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*append-only*");
    }
}
