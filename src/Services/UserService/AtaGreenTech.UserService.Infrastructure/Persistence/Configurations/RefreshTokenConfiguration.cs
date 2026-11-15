using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(RefreshToken.TokenHashMaxLength);
        builder.Property(x => x.CreatedByIp).HasMaxLength(RefreshToken.CreatedByIpMaxLength);
        builder.Property(x => x.DeviceInfo).HasMaxLength(RefreshToken.DeviceInfoMaxLength);

        builder.HasIndex(x => x.TokenHash).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);
        builder.HasIndex(x => x.FamilyId);
        builder.HasIndex(x => new { x.UserId, x.IsRevoked });
    }
}
