using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.CreateUnitOfMeasure;

internal sealed class CreateUnitOfMeasureCommandHandler(
    IRepository<UnitOfMeasure> unitsOfMeasure,
    IOptions<LocalizationOptions> localization
) : ICommandHandler<CreateUnitOfMeasureCommand, Guid>
{
    public async Task<Guid> Handle(CreateUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var code = UnitOfMeasure.NormalizeCode(request.Code);

        CheckRule.ThrowIf(
            new ValueMustBeUniqueRule(await unitsOfMeasure.ExistsAsync(u => u.Code == code, cancellationToken)),
            CatalogErrors.UnitOfMeasureCodeAlreadyExists,
            code
        );

        var unitOfMeasure = await unitsOfMeasure.AddAsync(
            new UnitOfMeasure(code, request.Translations.ToEntities(), localization.Value.DefaultLanguage),
            cancellationToken
        );

        return unitOfMeasure.Id;
    }
}
