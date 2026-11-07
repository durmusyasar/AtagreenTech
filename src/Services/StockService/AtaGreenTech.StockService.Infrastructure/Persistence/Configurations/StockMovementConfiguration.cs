using AtaGreenTech.StockService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Configurations;

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    private const int TypeMaxLength = 16;

    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable(
            "StockMovements",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_StockMovements_Quantity_Positive",
                    $"\"{nameof(StockMovement.Quantity)}\" > 0"
                );

                // Giriş yalnızca hedef, çıkış yalnızca kaynak, transfer iki farklı depo içerir.
                table.HasCheckConstraint(
                    "CK_StockMovements_Warehouses",
                    $"""
                    ("{nameof(StockMovement.Type)}" = '{MovementType.Inbound}' AND "{nameof(StockMovement.SourceWarehouseId)}" IS NULL AND "{nameof(StockMovement.TargetWarehouseId)}" IS NOT NULL)
                    OR ("{nameof(StockMovement.Type)}" = '{MovementType.Outbound}' AND "{nameof(StockMovement.SourceWarehouseId)}" IS NOT NULL AND "{nameof(StockMovement.TargetWarehouseId)}" IS NULL)
                    OR ("{nameof(StockMovement.Type)}" = '{MovementType.Transfer}' AND "{nameof(StockMovement.SourceWarehouseId)}" IS NOT NULL AND "{nameof(StockMovement.TargetWarehouseId)}" IS NOT NULL AND "{nameof(StockMovement.SourceWarehouseId)}" <> "{nameof(StockMovement.TargetWarehouseId)}")
                    """
                );
            }
        );
        builder.HasKey(x => x.Id);

        // Denetim kayıtlarında okunabilir olması için hareket tipi adıyla saklanır.
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(TypeMaxLength);

        builder.Property(x => x.Quantity).HasPrecision(18, StockBalance.QuantityScale);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.DocumentReference).HasMaxLength(StockMovement.DocumentReferenceMaxLength);
        builder.Property(x => x.Description).HasMaxLength(StockMovement.DescriptionMaxLength);
        builder.Property(x => x.ClientIpAddress).HasMaxLength(StockMovement.ClientIpAddressMaxLength);
        builder.Property(x => x.CorrelationId).HasMaxLength(StockMovement.CorrelationIdMaxLength);

        // Hareket geçmişi en yeniden eskiye listelenir.
        builder.HasIndex(x => x.CreatedAt).IsDescending();
        builder.HasIndex(x => new { x.ProductId, x.CreatedAt }).IsDescending(false, true);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceWarehouse)
            .WithMany()
            .HasForeignKey(x => x.SourceWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetWarehouse)
            .WithMany()
            .HasForeignKey(x => x.TargetWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
