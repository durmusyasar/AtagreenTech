using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(Role.NameMaxLength);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(Role.DescriptionMaxLength);

        builder.HasIndex(x => x.Name).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);

        builder.HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.UserRoles).HasField("_userRoles").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
