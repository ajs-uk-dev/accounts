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

        b.Property(x => x.FirmId)
            .HasConversion(v => v.Value, v => new FirmId(v))
            .HasColumnName("firm_id").IsRequired();

        b.Property(x => x.ActorUserId)
            .HasConversion(
                v => v!.Value.Value,
                v => new UserId(v))
            .HasColumnName("actor_user_id");

        b.Property(x => x.Action).HasConversion<string>().HasMaxLength(64).IsRequired();
        b.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
        b.Property(x => x.EntityId).HasMaxLength(128).IsRequired();
        b.Property(x => x.Payload).HasColumnType("jsonb");
        b.Property(x => x.CorrelationId).HasMaxLength(64);
        b.Property(x => x.OccurredAt).IsRequired();

        b.HasIndex(x => new { x.FirmId, x.OccurredAt });
        b.HasIndex(x => new { x.FirmId, x.Action, x.OccurredAt });
    }
}
