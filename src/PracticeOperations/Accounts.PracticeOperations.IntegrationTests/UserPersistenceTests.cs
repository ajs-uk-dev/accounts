using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using Accounts.SharedKernel.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Accounts.PracticeOperations.IntegrationTests;

[Collection(nameof(PostgresCollection))]
public class UserPersistenceTests
{
    private readonly PostgresFixture _pg;
    public UserPersistenceTests(PostgresFixture pg) => _pg = pg;

    private sealed class FakeFirmContext : IFirmContext
    {
        public FirmId? FirmId { get; set; }
        public UserId? UserId { get; set; }
        public bool IsAuthenticated => FirmId.HasValue;
    }

    private static string UniqueSlug(string prefix)
    {
        var s = $"{prefix}-{Guid.NewGuid():N}";
        return s.Length > 24 ? s.Substring(0, 24) : s;
    }

    private static string UniqueLocalPart(string prefix) =>
        $"{prefix}-{Guid.NewGuid():N}".ToLowerInvariant();

    [Fact]
    public async Task Round_trips_user_through_db()
    {
        var ctx = new FakeFirmContext();
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        var now = new DateTimeOffset(2026, 5, 11, 12, 0, 0, TimeSpan.Zero);
        var firm = Firm.Register("Round-trip Firm", UniqueSlug("rt"), now);
        var rawEmail = $"  Alice.{UniqueLocalPart("user")}@Acme.CO.uk  ";
        var email = EmailAddress.Create(rawEmail).Value!;
        var user = User.Register(firm.Id, email, "hashed-password-value", Role.FirmOwner, now);
        var userId = user.Id;

        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = firm.Id;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Set<Firm>().Add(firm);
            db.Set<User>().Add(user);
            await db.SaveChangesAsync();
        }

        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = firm.Id;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            var loaded = await db.Set<User>().SingleAsync(u => u.Id == userId);

            loaded.FirmId.Should().Be(firm.Id);
            loaded.Email.Value.Should().Be(rawEmail.Trim().ToLowerInvariant());
            loaded.PasswordHash.Should().Be("hashed-password-value");
            loaded.Role.Should().Be(Role.FirmOwner);
            loaded.Status.Should().Be(UserStatus.PendingVerification);
            loaded.TotpEnrolled.Should().BeFalse();
            loaded.FailedSignInAttempts.Should().Be(0);
            loaded.CreatedAt.Should().Be(now);
            loaded.UpdatedAt.Should().Be(now);
        }
    }

    [Fact]
    public async Task Unique_email_per_firm_is_enforced()
    {
        var ctx = new FakeFirmContext();
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        var now = DateTimeOffset.UtcNow;
        var firm = Firm.Register("Unique-per-firm", UniqueSlug("uf"), now);
        var sharedEmail = EmailAddress.Create($"{UniqueLocalPart("alice")}@acme.co.uk").Value!;
        var first = User.Register(firm.Id, sharedEmail, "h1", Role.FirmOwner, now);
        var second = User.Register(firm.Id, sharedEmail, "h2", Role.FeeEarner, now);

        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = firm.Id;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Set<Firm>().Add(firm);
            db.Set<User>().Add(first);
            await db.SaveChangesAsync();
        }

        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = firm.Id;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Set<User>().Add(second);
            var act = () => db.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateException>();
        }
    }

    [Fact]
    public async Task Same_email_in_different_firms_is_allowed()
    {
        var ctx = new FakeFirmContext();
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        var now = DateTimeOffset.UtcNow;
        var firmX = Firm.Register("Firm X", UniqueSlug("fx"), now);
        var firmY = Firm.Register("Firm Y", UniqueSlug("fy"), now);
        var sharedEmail = EmailAddress.Create($"{UniqueLocalPart("alice")}@acme.co.uk").Value!;
        var userX = User.Register(firmX.Id, sharedEmail, "h1", Role.FirmOwner, now);
        var userY = User.Register(firmY.Id, sharedEmail, "h2", Role.FirmOwner, now);

        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = firmX.Id;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Set<Firm>().AddRange(firmX, firmY);
            db.Set<User>().Add(userX);
            await db.SaveChangesAsync();
        }

        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = firmY.Id;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Set<User>().Add(userY);
            await db.SaveChangesAsync();
        }

        using (var scope = api.Services.CreateScope())
        {
            // Bypass tenant filter to count both rows.
            ctx.FirmId = null;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            var count = await db.Set<User>()
                .IgnoreQueryFilters()
                .Where(u => u.Email == sharedEmail)
                .CountAsync();
            count.Should().Be(2);
        }
    }
}
