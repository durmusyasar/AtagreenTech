using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).IsRequired().HasMaxLength(AuditLog.EmailMaxLength);
        builder.Property(x => x.Action).IsRequired().HasMaxLength(AuditLog.ActionMaxLength);
        builder.Property(x => x.IpAddress).HasMaxLength(AuditLog.IpAddressMaxLength);
        builder.Property(x => x.Details).HasMaxLength(AuditLog.DetailsMaxLength);

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.Action);
        builder.HasIndex(x => x.Email);
        builder.HasIndex(x => new { x.Action, x.CreatedAt });
    }
}
