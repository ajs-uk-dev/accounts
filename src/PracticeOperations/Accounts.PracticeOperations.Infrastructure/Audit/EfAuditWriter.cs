using System.Text.Json;
using Accounts.PracticeOperations.Application.Abstractions;
using Accounts.PracticeOperations.Domain.Audit;
using Accounts.PracticeOperations.Infrastructure.Persistence;
using Accounts.SharedKernel.Identity;
using Accounts.SharedKernel.Time;
using Microsoft.AspNetCore.Http;

namespace Accounts.PracticeOperations.Infrastructure.Audit;

public sealed class EfAuditWriter : IAuditWriter
{
    private readonly PracticeOperationsDbContext _db;
    private readonly IFirmContext _ctx;
    private readonly IClock _clock;
    private readonly IHttpContextAccessor _http;

    public EfAuditWriter(PracticeOperationsDbContext db, IFirmContext ctx, IClock clock, IHttpContextAccessor http)
    {
        _db = db;
        _ctx = ctx;
        _clock = clock;
        _http = http;
    }

    public async Task RecordAsync(
        AuditAction action,
        string entityType,
        string entityId,
        string? payload = null,
        CancellationToken ct = default)
    {
        if (action == AuditAction.Unknown)
            throw new ArgumentOutOfRangeException(nameof(action), "AuditAction.Unknown must not be recorded.");

        var firmId = _ctx.FirmId
            ?? throw new InvalidOperationException("Audit write requires an authenticated firm context.");

        var correlationId = _http.HttpContext?.TraceIdentifier;

        var evt = AuditEvent.Record(
            firmId, _ctx.UserId, action, entityType, entityId, payload, correlationId, _clock.UtcNow);

        _db.Set<AuditEvent>().Add(evt);
        await _db.SaveChangesAsync(ct);
    }

    public async Task WriteExplicitAsync(
        AuditAction action,
        FirmId? firmId,
        UserId? actorUserId,
        string? subject,
        IReadOnlyDictionary<string, string>? metadata,
        CancellationToken ct = default)
    {
        if (action == AuditAction.Unknown)
            throw new ArgumentOutOfRangeException(nameof(action), "AuditAction.Unknown must not be recorded.");

        var correlationId = _http.HttpContext?.TraceIdentifier;
        var payload = metadata is not null ? JsonSerializer.Serialize(metadata) : null;

        var evt = AuditEvent.RecordExplicit(
            firmId, actorUserId, action,
            entityType: action.ToString(),
            entityId: firmId?.ToString() ?? subject ?? string.Empty,
            subject: subject,
            payload: payload,
            correlationId: correlationId,
            occurredAt: _clock.UtcNow);

        _db.Set<AuditEvent>().Add(evt);
        await _db.SaveChangesAsync(ct);
    }
}
