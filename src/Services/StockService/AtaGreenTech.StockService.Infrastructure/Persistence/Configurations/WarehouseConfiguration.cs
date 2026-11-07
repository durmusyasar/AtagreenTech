using AtaGreenTech.StockService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Configurations;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(Warehouse.NameMaxLength);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(Warehouse.CodeMaxLength);
        builder.Property(x => x.Address).HasMaxLength(Warehouse.AddressMaxLength);

        builder.HasIndex(x => x.Code).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);
    }
}
