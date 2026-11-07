using AtaGreenTech.StockService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Configurations;

internal sealed class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("UnitsOfMeasure");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).IsRequired().HasMaxLength(UnitOfMeasure.CodeMaxLength);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(UnitOfMeasure.NameMaxLength);

        builder.HasIndex(x => x.Code).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);
    }
}
