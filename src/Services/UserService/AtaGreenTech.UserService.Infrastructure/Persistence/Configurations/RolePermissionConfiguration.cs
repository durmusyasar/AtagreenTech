using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Configurations;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.RoleId, x.PermissionId })
            .IsUnique()
            .HasFilter(ConfigurationConstants.NotDeletedFilter);
    }
}
