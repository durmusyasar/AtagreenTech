using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Configurations;

internal sealed class UnitOfMeasureTranslationConfiguration : IEntityTypeConfiguration<UnitOfMeasureTranslation>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasureTranslation> builder)
    {
        builder.ToTable("UnitOfMeasureTranslations");
        builder.HasKey(x => x.Id);

        // Id domain'de üretilir; EF, mevcut bir birime eklenen çeviriyi Id dolu olduğu için UPDATE etmeye çalışmasın.
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.LanguageCode).IsRequired().HasMaxLength(LanguageCode.MaxLength);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(UnitOfMeasureTranslation.NameMaxLength);
        builder.Property(x => x.Symbol).IsRequired().HasMaxLength(UnitOfMeasureTranslation.SymbolMaxLength);

        // Silinen bir dil daha sonra yeniden eklenebilsin diye benzersizlik yalnızca silinmemiş kayıtlarda aranır.
        builder.HasIndex(x => new { x.UnitOfMeasureId, x.LanguageCode })
            .IsUnique()
            .HasFilter(ConfigurationConstants.NotDeletedFilter);
    }
}
