using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.PracticeOperations.IntegrationTests.Persistence;

internal sealed class TenantTestRowConfiguration : IEntityTypeConfiguration<TenantTestRow>
{
    public void Configure(EntityTypeBuilder<TenantTestRow> b)
    {
        b.ToTable("tenant_test_rows");
        b.HasKey(x => x.Id);
        b.Property(x => x.FirmId)
            .HasConversion(v => v.Value, v => new FirmId(v))
            .IsRequired();
        b.HasIndex(x => x.FirmId);
        b.Property(x => x.Label).HasMaxLength(200).IsRequired();
    }
}
