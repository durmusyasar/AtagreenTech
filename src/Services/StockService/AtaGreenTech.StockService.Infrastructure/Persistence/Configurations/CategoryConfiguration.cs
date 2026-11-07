using AtaGreenTech.StockService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).IsRequired().HasMaxLength(Category.CodeMaxLength);

        builder.HasIndex(x => x.Code).IsUnique().HasFilter(ConfigurationConstants.NotDeletedFilter);

        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.SubCategories)
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Çeviriler aggregate'in parçasıdır: kategori her yüklendiğinde birlikte gelir ve koleksiyondan çıkarılan
        // çeviri silinir (OmniSync bunu soft delete'e çevirir).
        builder.HasMany(x => x.Translations)
            .WithOne()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Translations).AutoInclude();
    }
}
