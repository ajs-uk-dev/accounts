using Accounts.SharedKernel.Domain;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain.Audit;

public sealed class AuditEvent : ITenantScopedEntity
{
    public Guid Id { get; private set; }
    public FirmId FirmId { get; private set; }
    public UserId? ActorUserId { get; private set; }
    public AuditAction Action { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string? Payload { get; private set; }
    public string? CorrelationId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private AuditEvent() { }

    public static AuditEvent Record(
        FirmId firmId,
        UserId? actorUserId,
        AuditAction action,
        string entityType,
        string entityId,
        string? payload,
        string? correlationId,
        DateTimeOffset occurredAt) => new()
        {
            Id = Guid.NewGuid(),
            FirmId = firmId,
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Payload = payload,
            CorrelationId = correlationId,
            OccurredAt = occurredAt,
        };
}
