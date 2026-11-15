using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Configurations;

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.RoleId })
            .IsUnique()
            .HasFilter(ConfigurationConstants.NotDeletedFilter);
    }
}
