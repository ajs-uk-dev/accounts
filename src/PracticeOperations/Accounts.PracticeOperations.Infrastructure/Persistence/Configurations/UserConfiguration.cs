using Accounts.PracticeOperations.Domain.Users;
using Accounts.SharedKernel.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accounts.PracticeOperations.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id)
            .HasConversion(v => v.Value, v => new UserId(v));
        b.Property(x => x.FirmId)
            .HasConversion(v => v.Value, v => new FirmId(v))
            .IsRequired();
        b.HasIndex(x => x.FirmId);

        b.Property(x => x.Email)
            .HasConversion(
                v => v.Value,
                v => EmailAddress.Create(v).Value!)
            .HasMaxLength(256).IsRequired();
        b.HasIndex(nameof(User.FirmId), nameof(User.Email)).IsUnique();

        b.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        b.Property(x => x.Role).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        b.Property(x => x.TotpEnrolled).IsRequired();
        b.Property(x => x.TotpSecret).HasMaxLength(128);
        b.Property(x => x.LastSignInAt);
        b.Property(x => x.FailedSignInAttempts).IsRequired();
        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt).IsRequired();
        b.Ignore(x => x.DomainEvents);
    }
}
