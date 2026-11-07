using AtaGreenTech.StockService.Application.Common;
using AtaGreenTech.StockService.Domain.Entities;
using AtaGreenTech.StockService.Domain.Errors;
using AtaGreenTech.StockService.Domain.Rules;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Common;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.UpdateUnitOfMeasure;

internal sealed class UpdateUnitOfMeasureCommandHandler(
    IRepository<UnitOfMeasure> unitsOfMeasure,
    IOptions<LocalizationOptions> localization
) : ICommandHandler<UpdateUnitOfMeasureCommand>
{
    public async Task Handle(UpdateUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        // Çeviriler birimle birlikte yüklenir (AutoInclude); silinen diller soft delete ile işaretlenir.
        var unitOfMeasure = await unitsOfMeasure.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(UnitOfMeasure), request.Id);

        var code = UnitOfMeasure.NormalizeCode(request.Code);

        CheckRule.ThrowIf(
            new ValueMustBeUniqueRule(
                await unitsOfMeasure.ExistsAsync(u => u.Code == code && u.Id != request.Id, cancellationToken)
            ),
            CatalogErrors.UnitOfMeasureCodeAlreadyExists,
            code
        );

        unitOfMeasure.Update(
            code,
            request.Translations.ToEntities(),
            localization.Value.DefaultLanguage,
            request.IsActive
        );
    }
}
