using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Queries.GetUnitsOfMeasure;

/// <summary>
/// Tüm ölçü birimleri; az sayıda referans kayıt olduğu için sayfalanmaz.
/// </summary>
/// <param name="Language">Ad ve sembolün gösterileceği dil; verilmezse sistemin varsayılan dili.</param>
public sealed record GetUnitsOfMeasureQuery(string? Language = null, bool? IsActive = null)
    : IQuery<IReadOnlyList<UnitOfMeasureDto>>;
