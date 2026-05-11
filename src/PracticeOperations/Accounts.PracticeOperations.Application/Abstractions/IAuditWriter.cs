using Accounts.PracticeOperations.Domain.Audit;

namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IAuditWriter
{
    Task RecordAsync(
        AuditAction action,
        string entityType,
        string entityId,
        string? payload = null,
        CancellationToken ct = default);
}
