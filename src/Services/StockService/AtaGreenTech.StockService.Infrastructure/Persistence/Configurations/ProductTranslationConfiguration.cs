using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Configurations;

internal sealed class ProductTranslationConfiguration : IEntityTypeConfiguration<ProductTranslation>
{
    public void Configure(EntityTypeBuilder<ProductTranslation> builder)
    {
        builder.ToTable("ProductTranslations");
        builder.HasKey(x => x.Id);

        // Id domain'de üretilir; EF, mevcut bir ürüne eklenen çeviriyi Id dolu olduğu için UPDATE etmeye çalışmasın.
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.LanguageCode).IsRequired().HasMaxLength(LanguageCode.MaxLength);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(ProductTranslation.NameMaxLength);
        builder.Property(x => x.Description).HasMaxLength(ProductTranslation.DescriptionMaxLength);

        // Silinen bir dil daha sonra yeniden eklenebilsin diye benzersizlik yalnızca silinmemiş kayıtlarda aranır.
        builder.HasIndex(x => new { x.ProductId, x.LanguageCode })
            .IsUnique()
            .HasFilter(ConfigurationConstants.NotDeletedFilter);
    }
}
