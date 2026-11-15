using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).IsRequired().HasMaxLength(User.EmailMaxLength);
        builder.Property(x => x.Username).IsRequired().HasMaxLength(User.UsernameMaxLength);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(User.PasswordHashMaxLength);
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(User.NameMaxLength);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(User.NameMaxLength);
        builder.Property(x => x.SecurityStamp).IsRequired().HasMaxLength(User.SecurityStampMaxLength);
        builder.Property(x => x.EmailVerificationTokenHash).HasMaxLength(User.EmailVerificationTokenHashMaxLength);
        builder.Property(x => x.PasswordResetTokenHash).HasMaxLength(User.PasswordResetTokenHashMaxLength);

        builder.HasIndex(x => x.Email).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);
        builder.HasIndex(x => x.Username).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);
        builder.HasIndex(x => x.EmailVerificationTokenHash)
            .IsUnique()
            .HasFilter($"\"{nameof(User.EmailVerificationTokenHash)}\" IS NOT NULL");
        builder.HasIndex(x => x.PasswordResetTokenHash)
            .IsUnique()
            .HasFilter($"\"{nameof(User.PasswordResetTokenHash)}\" IS NOT NULL");

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.UserRoles).HasField("_userRoles").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.RefreshTokens).HasField("_refreshTokens").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
