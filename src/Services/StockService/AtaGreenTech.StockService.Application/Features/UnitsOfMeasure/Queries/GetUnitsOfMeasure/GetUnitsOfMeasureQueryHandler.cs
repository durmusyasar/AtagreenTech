using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Queries.GetUnitsOfMeasure;

internal sealed class GetUnitsOfMeasureQueryHandler(IStockDbContext context)
    : IQueryHandler<GetUnitsOfMeasureQuery, IReadOnlyList<UnitOfMeasureDto>>
{
    public async Task<IReadOnlyList<UnitOfMeasureDto>> Handle(
        GetUnitsOfMeasureQuery request,
        CancellationToken cancellationToken
    ) =>
        await context.UnitsOfMeasure
            .OrderBy(u => u.Code)
            .Select(u => new UnitOfMeasureDto(u.Id, u.Code, u.Name))
            .ToListAsync(cancellationToken);
}
