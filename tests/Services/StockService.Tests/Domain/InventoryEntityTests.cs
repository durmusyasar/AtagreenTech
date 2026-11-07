using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using OmniSync.Core.Domain.Exceptions;

namespace AtaGreenTech.StockService.Tests.Domain;

public class InventoryEntityTests
{
    private static readonly MovementOrigin Origin = new("10.0.0.5", "trace-1");

    [Fact]
    public void Warehouse_normalizes_code_and_text_fields()
    {
        var warehouse = new Warehouse(" Merkez Depo ", " mtr-01 ", "  ");

        Assert.Equal("Merkez Depo", warehouse.Name);
        Assert.Equal("MTR-01", warehouse.Code);
        Assert.Null(warehouse.Address);
        Assert.True(warehouse.IsActive);
    }

    [Fact]
    public void Balance_increases_and_decreases()
    {
        var balance = new StockBalance(Guid.NewGuid(), Guid.NewGuid());

        balance.Increase(10.5m);
        balance.Decrease(10.5m);

        Assert.Equal(0m, balance.Quantity);
    }

    [Fact]
    public void Balance_cannot_go_negative()
    {
        var balance = new StockBalance(Guid.NewGuid(), Guid.NewGuid());
        balance.Increase(5m);

        var exception = Assert.Throws<ApplicationValidationExceptionWithResourceKey>(() => balance.Decrease(5.0001m));

        Assert.Equal(InventoryErrors.InsufficientStock, exception.ResourceKey);
        Assert.Equal(5m, balance.Quantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Balance_rejects_non_positive_amounts(decimal amount)
    {
        var balance = new StockBalance(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(() => balance.Increase(amount));
        Assert.Throws<ArgumentOutOfRangeException>(() => balance.Decrease(amount));
    }

    [Fact]
    public void Movement_factories_set_warehouses_by_type()
    {
        var productId = Guid.NewGuid();
        var source = Guid.NewGuid();
        var target = Guid.NewGuid();

        var inbound = StockMovement.Inbound(productId, target, 1m, 2m, " FT-1 ", null, Origin);
        var outbound = StockMovement.Outbound(productId, source, 1m, 2m, null, null, Origin);
        var transfer = StockMovement.Transfer(productId, source, target, 1m, 2m, null, null, Origin);

        Assert.Equal((MovementType.Inbound, (Guid?)null, (Guid?)target), (inbound.Type, inbound.SourceWarehouseId, inbound.TargetWarehouseId));
        Assert.Equal((MovementType.Outbound, (Guid?)source, (Guid?)null), (outbound.Type, outbound.SourceWarehouseId, outbound.TargetWarehouseId));
        Assert.Equal((MovementType.Transfer, (Guid?)source, (Guid?)target), (transfer.Type, transfer.SourceWarehouseId, transfer.TargetWarehouseId));
        Assert.Equal("FT-1", inbound.DocumentReference);
        Assert.Equal(("10.0.0.5", "trace-1"), (inbound.ClientIpAddress, inbound.CorrelationId));
    }

    [Fact]
    public void Transfer_requires_different_warehouses()
    {
        var warehouseId = Guid.NewGuid();

        var exception = Assert.Throws<ApplicationValidationExceptionWithResourceKey>(
            () => StockMovement.Transfer(Guid.NewGuid(), warehouseId, warehouseId, 1m, 0m, null, null, Origin)
        );

        Assert.Equal(InventoryErrors.TransferWarehousesMustDiffer, exception.ResourceKey);
    }

    [Fact]
    public void Movement_rejects_non_positive_quantity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => StockMovement.Inbound(Guid.NewGuid(), Guid.NewGuid(), 0m, 0m, null, null, Origin)
        );
    }
}
