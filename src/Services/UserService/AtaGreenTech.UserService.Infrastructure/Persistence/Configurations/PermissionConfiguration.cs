using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(Permission.NameMaxLength);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(Permission.DescriptionMaxLength);

        builder.HasIndex(x => x.Name).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);

        builder.HasMany(x => x.RolePermissions)
            .WithOne(x => x.Permission)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.RolePermissions)
            .HasField("_rolePermissions")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
