using Accounts.PracticeOperations.Domain.Audit;

namespace Accounts.PracticeOperations.Application.Behaviors;

/// <summary>Commands that produce an audit entry after they complete successfully.</summary>
public interface IAuditedCommand
{
    AuditAction Action { get; }
    string EntityType { get; }
    string EntityId { get; }
    string? Payload => null;
}
