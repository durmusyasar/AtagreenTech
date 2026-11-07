using AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateOutboundMovement;
using AtaGreenTech.StockService.Application.Features.StockMovements.Commands.CreateTransferMovement;
using AtaGreenTech.StockService.Application.Features.StockMovements.Queries.GetStockMovements;
using AtaGreenTech.StockService.Application.Features.Warehouses.Commands.CreateWarehouse;
using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Tests.Application;

public class InventoryValidatorTests
{
    private static CreateOutboundMovementCommand ValidOutbound() =>
        new(Guid.NewGuid(), Guid.NewGuid(), 2.5m, 10m, "IRS-2026-001", null);

    [Fact]
    public void Valid_outbound_command_passes()
    {
        Assert.True(new CreateOutboundMovementCommandValidator().Validate(ValidOutbound()).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.00001)]
    public void Invalid_quantity_fails(decimal quantity)
    {
        var result = new CreateOutboundMovementCommandValidator().Validate(ValidOutbound() with { Quantity = quantity });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateOutboundMovementCommand.Quantity));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.001)]
    public void Invalid_unit_price_fails(decimal unitPrice)
    {
        var result = new CreateOutboundMovementCommandValidator().Validate(ValidOutbound() with { UnitPrice = unitPrice });

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateOutboundMovementCommand.UnitPrice));
    }

    [Fact]
    public void Too_long_document_reference_fails()
    {
        var command = ValidOutbound() with
        {
            DocumentReference = new string('x', StockMovement.DocumentReferenceMaxLength + 1),
        };

        var result = new CreateOutboundMovementCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateOutboundMovementCommand.DocumentReference));
    }

    [Fact]
    public void Transfer_to_same_warehouse_fails()
    {
        var warehouseId = Guid.NewGuid();
        var command = new CreateTransferMovementCommand(warehouseId, warehouseId, Guid.NewGuid(), 1m, 0m, null, null);

        var result = new CreateTransferMovementCommandValidator().Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateTransferMovementCommand.TargetWarehouseId));
    }

    [Theory]
    [InlineData("MTR-01", true)]
    [InlineData("MTR 01", false)]
    [InlineData("", false)]
    public void Warehouse_code_format_is_validated(string code, bool isValid)
    {
        var result = new CreateWarehouseCommandValidator().Validate(new CreateWarehouseCommand("Merkez", code, null));

        Assert.Equal(isValid, result.IsValid);
    }

    [Fact]
    public void Movement_history_date_range_must_be_ordered()
    {
        var now = DateTimeOffset.UtcNow;

        var result = new GetStockMovementsQueryValidator().Validate(new GetStockMovementsQuery(From: now, To: now.AddDays(-1)));

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetStockMovementsQuery.To));
    }
}
