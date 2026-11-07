using AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.CreateUnitOfMeasure;
using AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Queries.GetUnitsOfMeasure;
using MediatR;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.StockService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/units-of-measure</c>: ölçü birimleri.
/// </summary>
public sealed class UnitOfMeasureEndpoints : IEndpointModule
{
    public const string ModuleName = "units-of-measure";

    public string Name => ModuleName;

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("", (ISender sender, CancellationToken cancellationToken) =>
            sender.Send(new GetUnitsOfMeasureQuery(), cancellationToken));

        group.MapPost("", async (CreateUnitOfMeasureCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var id = await sender.Send(command, cancellationToken);
            return TypedResults.Created((string?)null, id);
        });
    }
}
