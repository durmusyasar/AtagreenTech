using AtaGreenTech.StockService.Application.Common;
using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.Warehouses.Queries.GetWarehouses;

internal sealed class GetWarehousesQueryValidator : AbstractValidator<GetWarehousesQuery>
{
    public GetWarehousesQueryValidator()
    {
        RuleFor(x => x.PageNumber).ValidPageNumber();
        RuleFor(x => x.PageSize).ValidPageSize();
    }
}
