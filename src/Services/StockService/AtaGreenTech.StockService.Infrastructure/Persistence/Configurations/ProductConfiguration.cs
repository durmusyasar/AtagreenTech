using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Pricing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Sku).IsRequired().HasMaxLength(Product.SkuMaxLength);
        builder.Property(x => x.Barcode).HasMaxLength(Gtin.MaxLength);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.Currency).IsRequired().HasMaxLength(CurrencyCode.Length).IsFixedLength();

        // Uygulamadaki benzersizlik kontrolü eş zamanlı isteklerde yarışabilir; son güvence bu index'lerdir.
        builder.HasIndex(x => x.Sku).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);
        builder.HasIndex(x => x.Barcode).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);

        // Restrict yalnızca fiziksel silmeyi engeller; soft delete'te ürünü olan kategori veya birimin silinmemesi
        // application katmanında kontrol edilmelidir.
        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(x => x.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);

        // Çeviriler aggregate'in parçasıdır: ürün her yüklendiğinde birlikte gelir; koleksiyondan çıkarılan çeviri ve
        // silinen ürünün çevirileri silinir (OmniSync bunu soft delete'e çevirir).
        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Translations).AutoInclude();
    }
}
