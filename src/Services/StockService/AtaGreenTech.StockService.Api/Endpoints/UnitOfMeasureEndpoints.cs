using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.StockService.Application.DTOs;
using AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.CreateUnitOfMeasure;
using AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Commands.UpdateUnitOfMeasure;
using AtaGreenTech.StockService.Application.Features.UnitsOfMeasure.Queries.GetUnitsOfMeasure;
using MediatR;
using AtaGreenTech.StockService.Api.Security;
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
        group.RequireModule(ModuleCodes.Stock);

        group.MapGet("", (
            ISender sender,
            CancellationToken cancellationToken,
            string? language = null,
            bool? isActive = null
        ) => sender.Send(new GetUnitsOfMeasureQuery(language, isActive), cancellationToken))
            .RequireAuthorization(AuthorizationPolicies.StockRead);

        group.MapPost("", async (CreateUnitOfMeasureCommand command, ISender sender, CancellationToken cancellationToken) =>
        {
            var id = await sender.Send(command, cancellationToken);
            return TypedResults.Created((string?)null, id);
        }).RequireAuthorization(AuthorizationPolicies.StockWrite);

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateUnitOfMeasureRequest request,
            ISender sender,
            CancellationToken cancellationToken
        ) =>
        {
            await sender.Send(
                new UpdateUnitOfMeasureCommand(id, request.Code, request.Translations, request.IsActive),
                cancellationToken
            );
            return TypedResults.NoContent();
        }).RequireAuthorization(AuthorizationPolicies.StockWrite);
    }
}


public sealed record UpdateUnitOfMeasureRequest(
    string Code,
    IReadOnlyList<UnitOfMeasureTranslationDto> Translations,
    bool IsActive
);
