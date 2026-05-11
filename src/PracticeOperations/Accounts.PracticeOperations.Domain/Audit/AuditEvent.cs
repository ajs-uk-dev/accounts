using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Domain.Audit;

/// <summary>
/// Immutable record of a system event for auditing purposes.
/// <para>
/// AuditEvent is NOT tenant-scoped (does not implement ITenantScopedEntity) because some
/// events (e.g. UserSignInFailed when the email is unknown) have no associated firm.
/// Queries that need firm-scoping must add an explicit WHERE predicate.
/// </para>
/// </summary>
public sealed class AuditEvent
{
    public Guid Id { get; private set; }

    /// <summary>
    /// Nullable to support pre-authentication events (e.g. UserSignInFailed when the email
    /// does not match any user — no firm context is available at that point).
    /// Invariant: at least one of <see cref="FirmId"/> or <see cref="Subject"/> must be non-null.
    /// </summary>
    public FirmId? FirmId { get; private set; }

    public UserId? ActorUserId { get; private set; }
    public AuditAction Action { get; private set; }
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;

    /// <summary>
    /// Free-text subject identifier used when no actor user is known (e.g. the attempted
    /// e-mail address in a UserSignInFailed / UserNotFound event).  Max 320 chars (RFC 5321).
    /// </summary>
    public string? Subject { get; private set; }

    public string? Payload { get; private set; }
    public string? CorrelationId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }

    private AuditEvent() { }

    /// <summary>Records an authenticated-context audit event (firm and user are known).</summary>
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

    /// <summary>
    /// Records an explicit-context audit event where the firm and/or user IDs are provided
    /// directly (used for pre-authentication events such as FirmRegistered and SignIn flows).
    /// At least one of <paramref name="firmId"/> or <paramref name="subject"/> must be non-null.
    /// </summary>
    public static AuditEvent RecordExplicit(
        FirmId? firmId,
        UserId? actorUserId,
        AuditAction action,
        string entityType,
        string entityId,
        string? subject,
        string? payload,
        string? correlationId,
        DateTimeOffset occurredAt)
    {
        if (firmId is null && subject is null)
            throw new ArgumentException(
                "At least one of firmId or subject must be non-null for an audit event.");

        return new AuditEvent
        {
            Id = Guid.NewGuid(),
            FirmId = firmId,
            ActorUserId = actorUserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Subject = subject,
            Payload = payload,
            CorrelationId = correlationId,
            OccurredAt = occurredAt,
        };
    }
}
