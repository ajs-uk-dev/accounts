using Accounts.PracticeOperations.Domain.Audit;
using Accounts.SharedKernel.Identity;

namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IAuditWriter
{
    /// <summary>
    /// Records an audit event using the ambient firm/user context (requires an authenticated
    /// <see cref="IFirmContext"/>).  Throws if <c>IFirmContext.FirmId</c> is null.
    /// </summary>
    Task RecordAsync(
        AuditAction action,
        string entityType,
        string entityId,
        string? payload = null,
        CancellationToken ct = default);

    /// <summary>
    /// Records an audit event with explicitly-supplied IDs, for use in pre-authentication
    /// flows (e.g. <c>RegisterFirm</c>, <c>SignIn</c>) where no ambient firm context exists.
    /// At least one of <paramref name="firmId"/> or <paramref name="subject"/> must be non-null.
    /// </summary>
    Task WriteExplicitAsync(
        AuditAction action,
        FirmId? firmId,
        UserId? actorUserId,
        string? subject,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken ct = default);
}
