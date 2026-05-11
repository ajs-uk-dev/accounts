using System.Net.Http.Json;
using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.PracticeOperations.Domain.Audit;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.PracticeOperations.IntegrationTests.Audit;

[Collection(nameof(PostgresCollection))]
public class FirmRegistrationAuditTests
{
    private readonly PostgresFixture _pg;
    public FirmRegistrationAuditTests(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task RegisterFirm_emits_FirmRegistered_audit_row_with_firm_and_owner_ids()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();

        var slug = $"far-{Guid.NewGuid():N}"[..12];
        var email = $"far-{Guid.NewGuid():N}@example.com";

        var response = await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("Audit Test Firm", slug, email, "long-enough-password"));

        response.IsSuccessStatusCode.Should().BeTrue("registration should succeed");
        var result = await response.Content.ReadFromJsonAsync<RegisterFirmResult>();
        result.Should().NotBeNull();

        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();

        var auditRow = await db.Set<AuditEvent>()
            .Where(e => e.FirmId == new Accounts.SharedKernel.Identity.FirmId(result!.FirmId)
                        && e.Action == AuditAction.FirmRegistered)
            .SingleOrDefaultAsync();

        auditRow.Should().NotBeNull("RegisterFirm must emit a FirmRegistered audit event");
        auditRow!.FirmId!.Value.Value.Should().Be(result!.FirmId,
            "audit row firm_id should match the newly-registered firm");
        auditRow.ActorUserId!.Value.Value.Should().Be(result.OwnerUserId,
            "audit row actor_user_id should be the owner's UserId");
        auditRow.Action.Should().Be(AuditAction.FirmRegistered);
    }
}
