using AtaGreenTech.StockService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Configurations;

internal sealed class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.ToTable(
            "StockBalances",
            table => table.HasCheckConstraint(
                "CK_StockBalances_Quantity_NonNegative",
                $"\"{nameof(StockBalance.Quantity)}\" >= 0"
            )
        );
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity).HasPrecision(18, StockBalance.QuantityScale);

        // Npgsql uint satır sürümünü PostgreSQL'in her güncellemede değişen xmin sistem sütununa eşler (migration'da
        // sütun oluşmaz). Kilit atlanarak bayat bir kopya üzerinden yapılan güncelleme eşzamanlılık hatasıyla reddedilir.
        builder.Property<uint>("RowVersion").IsRowVersion();

        // StockBalanceLocker'daki "ON CONFLICT" ifadesi bu index'in sütunlarına ve filtresine dayanır.
        builder.HasIndex(x => new { x.WarehouseId, x.ProductId })
            .IsUnique()
            .HasFilter(ConfigurationConstants.NotDeletedFilter);

        builder.HasIndex(x => x.ProductId);

        builder.HasOne(x => x.Warehouse)
            .WithMany()
            .HasForeignKey(x => x.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
