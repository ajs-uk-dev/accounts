using System.Net.Http.Json;
using System.Text.Json;
using Accounts.PracticeOperations.Application.Firms.Register;
using Accounts.PracticeOperations.Application.Users.SignIn;
using Accounts.PracticeOperations.Domain.Audit;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.PracticeOperations.IntegrationTests.Fixtures;
using Accounts.SharedKernel.Identity;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Accounts.PracticeOperations.IntegrationTests.Audit;

[Collection(nameof(PostgresCollection))]
public class SignInAuditTests
{
    private readonly PostgresFixture _pg;
    public SignInAuditTests(PostgresFixture pg) => _pg = pg;

    private static string UniqueSlug(string prefix)
    {
        var s = $"{prefix}-{Guid.NewGuid():N}";
        return s.Length > 20 ? s[..20] : s;
    }

    [Fact]
    public async Task Successful_sign_in_emits_UserSignedIn_audit_row()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var slug = UniqueSlug("si");
        var email = $"si-{Guid.NewGuid():N}@example.com";
        const string pwd = "long-enough-password";

        // Register firm to get firm + user IDs
        var regResp = await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("SI Firm", slug, email, pwd));
        regResp.IsSuccessStatusCode.Should().BeTrue();
        var reg = await regResp.Content.ReadFromJsonAsync<RegisterFirmResult>();

        // Sign in
        var signInResp = await client.PostAsJsonAsync("/api/auth/sign-in",
            new SignInCommand(email, pwd, null));
        signInResp.IsSuccessStatusCode.Should().BeTrue("sign-in should succeed");

        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();

        var auditRow = await db.Set<AuditEvent>()
            .Where(e => e.FirmId == new FirmId(reg!.FirmId)
                        && e.Action == AuditAction.UserSignedIn)
            .SingleOrDefaultAsync();

        auditRow.Should().NotBeNull("successful sign-in must emit a UserSignedIn audit event");
        auditRow!.ActorUserId!.Value.Value.Should().Be(reg!.OwnerUserId,
            "actor should be the signing-in user");
        auditRow.FirmId!.Value.Value.Should().Be(reg.FirmId);
    }

    [Fact]
    public async Task SignIn_with_unknown_email_emits_UserSignInFailed_with_null_firm_and_actor()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var attemptedEmail = $"nobody-{Guid.NewGuid():N}@example.com";

        var signInResp = await client.PostAsJsonAsync("/api/auth/sign-in",
            new SignInCommand(attemptedEmail, "any-password", null));
        signInResp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();

        // Filter by Subject since FirmId is null for unknown-email failures
        var auditRow = await db.Set<AuditEvent>()
            .Where(e => e.Action == AuditAction.UserSignInFailed
                        && e.Subject == attemptedEmail)
            .SingleOrDefaultAsync();

        auditRow.Should().NotBeNull("sign-in with unknown email must emit a UserSignInFailed audit event");
        auditRow!.FirmId.Should().BeNull("no firm context is available when the email is not found");
        auditRow.ActorUserId.Should().BeNull("no user is known when the email is not found");
        auditRow.Subject.Should().Be(attemptedEmail);

        // Verify Reason metadata
        auditRow.Payload.Should().NotBeNullOrEmpty();
        var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(auditRow.Payload!);
        payload.Should().ContainKey("Reason").WhoseValue.Should().Be("UserNotFound");
    }

    [Fact]
    public async Task SignIn_with_bad_password_emits_UserSignInFailed_with_firm_and_actor()
    {
        await using var api = new ApiFactory(_pg.ConnectionString);
        var client = api.CreateClient();
        var slug = UniqueSlug("bp");
        var email = $"bp-{Guid.NewGuid():N}@example.com";

        var regResp = await client.PostAsJsonAsync("/api/firms/register",
            new RegisterFirmCommand("BP Firm", slug, email, "correct-password"));
        regResp.IsSuccessStatusCode.Should().BeTrue();
        var reg = await regResp.Content.ReadFromJsonAsync<RegisterFirmResult>();

        // Sign in with wrong password
        var signInResp = await client.PostAsJsonAsync("/api/auth/sign-in",
            new SignInCommand(email, "wrong-password", null));
        signInResp.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);

        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PracticeOperationsDbContext>();

        var auditRow = await db.Set<AuditEvent>()
            .Where(e => e.FirmId == new FirmId(reg!.FirmId)
                        && e.Action == AuditAction.UserSignInFailed)
            .SingleOrDefaultAsync();

        auditRow.Should().NotBeNull("sign-in with bad password must emit a UserSignInFailed audit event");
        auditRow!.ActorUserId!.Value.Value.Should().Be(reg!.OwnerUserId,
            "actor should be the user who attempted to sign in");
        auditRow.FirmId!.Value.Value.Should().Be(reg.FirmId);

        auditRow.Payload.Should().NotBeNullOrEmpty();
        var payload = JsonSerializer.Deserialize<Dictionary<string, string>>(auditRow.Payload!);
        payload.Should().ContainKey("Reason").WhoseValue.Should().Be("BadPassword");
    }
}
