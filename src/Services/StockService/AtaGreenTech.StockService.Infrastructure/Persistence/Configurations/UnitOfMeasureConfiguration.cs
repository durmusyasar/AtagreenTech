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

        builder.HasIndex(x => x.Code).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);

        // Çeviriler aggregate'in parçasıdır: birim her yüklendiğinde birlikte gelir ve koleksiyondan çıkarılan
        // çeviri silinir (OmniSync bunu soft delete'e çevirir).
        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Translations).AutoInclude();
    }
}
