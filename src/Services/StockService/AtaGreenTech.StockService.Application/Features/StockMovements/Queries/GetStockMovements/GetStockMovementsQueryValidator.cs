using FluentValidation;

namespace AtaGreenTech.StockService.Application.Features.StockMovements.Queries.GetStockMovements;

internal sealed class GetStockMovementsQueryValidator : AbstractValidator<GetStockMovementsQuery>
{
    public GetStockMovementsQueryValidator()
    {
        RuleFor(x => x.Type).IsInEnum().WithMessage("Geçersiz hareket tipi.");

        RuleFor(x => x.To)
            .GreaterThan(x => x.From)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
    }
}
