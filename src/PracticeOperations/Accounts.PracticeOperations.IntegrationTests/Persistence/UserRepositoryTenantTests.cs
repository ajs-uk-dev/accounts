using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Firms;
using Accounts.PracticeOperations.Domain.Users;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using Accounts.SharedKernel.Identity;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Accounts.PracticeOperations.IntegrationTests.Persistence;

/// <summary>
/// Verifies that <see cref="IUserRepository.GetAsync"/> honours the tenant query
/// filter and does not leak users across firm boundaries.
/// </summary>
[Collection(nameof(PostgresCollection))]
public class UserRepositoryTenantTests
{
    private readonly PostgresFixture _pg;
    public UserRepositoryTenantTests(PostgresFixture pg) => _pg = pg;

    private sealed class FakeFirmContext : IFirmContext
    {
        public FirmId? FirmId { get; set; }
        public UserId? UserId { get; set; }
        public bool IsAuthenticated => FirmId.HasValue;
    }

    private static string UniqueSlug(string prefix)
    {
        var s = $"{prefix}-{Guid.NewGuid():N}";
        return s.Length > 24 ? s[..24] : s;
    }

    [Fact]
    public async Task GetAsync_returns_null_when_user_belongs_to_another_firm()
    {
        var ctx = new FakeFirmContext();
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        var now = DateTimeOffset.UtcNow;
        var firmA = Firm.Register("Firm A", UniqueSlug("fa"), now);
        var firmB = Firm.Register("Firm B", UniqueSlug("fb"), now);

        var emailA = EmailAddress.Create($"a-{Guid.NewGuid():N}@example.com").Value!;
        var emailB = EmailAddress.Create($"b-{Guid.NewGuid():N}@example.com").Value!;

        var userA = User.Register(firmA.Id, emailA, "hash-a", Role.FirmOwner, now);
        var userB = User.Register(firmB.Id, emailB, "hash-b", Role.FirmOwner, now);

        // Seed both users (bypass filter by leaving FirmId null).
        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = null;
            var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();
            db.Set<Firm>().AddRange(firmA, firmB);
            db.Set<User>().AddRange(userA, userB);
            await db.SaveChangesAsync();
        }

        // Set context to firm A, then try to look up firm B's user via the repository.
        using (var scope = api.Services.CreateScope())
        {
            ctx.FirmId = firmA.Id;
            var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var result = await repo.GetAsync(userB.Id);
            result.Should().BeNull(
                "GetAsync must honour the tenant filter and not return a user from another firm");
        }
    }

    [Fact]
    public async Task GetAsync_returns_user_when_user_belongs_to_the_current_firm()
    {
        var ctx = new FakeFirmContext();
        await using var api = new ApiFactory(_pg.ConnectionString, services =>
        {
            services.RemoveAll<IFirmContext>();
            services.AddSingleton<IFirmContext>(ctx);
        });

        var now = DateTimeOffset.UtcNow;
        var firm = Firm.Register("Firm Own", UniqueSlug("fo"), now);
        var email = EmailAddress.Create($"c-{Guid.NewGuid():N}@example.com").Value!;
        var user = User.Register(firm.Id, email, "hash-c", Role.FirmOwner, now);

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
            var repo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var result = await repo.GetAsync(user.Id);
            result.Should().NotBeNull("GetAsync should return the user when the firm context matches");
            result!.Id.Should().Be(user.Id);
        }
    }
}
