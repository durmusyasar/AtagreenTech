using AtaGreenTech.StockService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(Product.NameMaxLength);
        builder.Property(x => x.Sku).IsRequired().HasMaxLength(Product.SkuMaxLength);
        builder.Property(x => x.Description).HasMaxLength(Product.DescriptionMaxLength);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);

        // Uygulamadaki benzersizlik kontrolü eş zamanlı isteklerde yarışabilir; son güvence bu index'tir.
        builder.HasIndex(x => x.Sku).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);

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
    }
}
