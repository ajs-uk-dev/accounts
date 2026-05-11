using Accounts.PracticeOperations.Domain.Audit;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Configurations;

internal sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> b)
    {
        b.ToTable("audit_events");
        b.HasKey(x => x.Id);

        // Nullable: pre-authentication events (e.g. FirmRegistered, UserSignInFailed with
        // unknown email) may have no firm context.
        b.Property(x => x.FirmId)
            .HasConversion(
                v => v.HasValue ? (Guid?)v.Value.Value : null,
                v => v.HasValue ? new FirmId(v.Value) : (FirmId?)null);

        b.Property(x => x.ActorUserId)
            .HasConversion(
                v => v!.Value.Value,
                v => new UserId(v));

        b.Property(x => x.Action).HasConversion<string>().HasMaxLength(64).IsRequired();
        b.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
        b.Property(x => x.EntityId).HasMaxLength(128).IsRequired();

        // Subject: free-text identifier for events where no authenticated user exists
        // (e.g. the attempted e-mail address on sign-in failure).  RFC 5321 max: 320 chars.
        b.Property(x => x.Subject).HasMaxLength(320);

        b.Property(x => x.Payload).HasColumnType("jsonb");
        b.Property(x => x.CorrelationId).HasMaxLength(64);
        b.Property(x => x.OccurredAt).IsRequired();

        b.HasIndex(x => new { x.FirmId, x.OccurredAt });
        b.HasIndex(x => new { x.FirmId, x.Action, x.OccurredAt });
    }
}
