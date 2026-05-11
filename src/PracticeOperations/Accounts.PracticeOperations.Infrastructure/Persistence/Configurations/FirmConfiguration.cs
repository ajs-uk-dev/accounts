using Accounts.PracticeOperations.Domain.Firms;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Configurations;

internal sealed class FirmConfiguration : IEntityTypeConfiguration<Firm>
{
    public void Configure(EntityTypeBuilder<Firm> b)
    {
        b.ToTable("firms");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasConversion(v => v.Value, v => new FirmId(v));
        b.Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Property(x => x.Slug).HasMaxLength(64).IsRequired();
        b.HasIndex(x => x.Slug).IsUnique();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();
        b.Ignore(x => x.DomainEvents);
    }
}
