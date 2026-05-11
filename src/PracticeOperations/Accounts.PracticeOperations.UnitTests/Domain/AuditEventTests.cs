using Accounts.PracticeOperations.Domain.Audit;
using Accounts.SharedKernel.Identity;
using FluentAssertions;

namespace Accounts.PracticeOperations.UnitTests.Domain;

public class AuditEventTests
{
    [Fact]
    public void Create_sets_all_fields_and_generates_id()
    {
        var firm = FirmId.New();
        var user = UserId.New();
        var now = DateTimeOffset.UtcNow;

        var e = AuditEvent.Record(
            firm, user, AuditAction.UserSignedIn,
            "User", user.Value.ToString(),
            payload: "{\"ip\":\"127.0.0.1\"}",
            correlationId: "corr-1",
            occurredAt: now);

        e.Id.Should().NotBe(Guid.Empty);
        e.FirmId.Should().Be(firm);
        e.ActorUserId.Should().Be(user);
        e.Action.Should().Be(AuditAction.UserSignedIn);
        e.EntityType.Should().Be("User");
        e.EntityId.Should().Be(user.Value.ToString());
        e.Payload.Should().Be("{\"ip\":\"127.0.0.1\"}");
        e.CorrelationId.Should().Be("corr-1");
        e.OccurredAt.Should().Be(now);
    }
}
