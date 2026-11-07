using AtaGreenTech.StockService.Application.DTOs;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Queries.GetUnitsOfMeasure;

/// <summary>
/// Tüm ölçü birimleri; az sayıda referans kayıt olduğu için sayfalanmaz.
/// </summary>
public sealed record GetUnitsOfMeasureQuery : IQuery<IReadOnlyList<UnitOfMeasureDto>>;
