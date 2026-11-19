using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Features.System.Commands.CreateSystemMenu;
using AtaGreenTech.UserService.Application.Features.System.Commands.PatchSystemMenu;
using AtaGreenTech.UserService.Application.Features.System.Commands.UpdateSystemMenu;
using AtaGreenTech.UserService.Application.Features.System.Commands.UpdateSystemModule;
using AtaGreenTech.UserService.Application.Features.System.DTOs;
using AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemLanguages;
using AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemMenuById;
using AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemMenus;
using AtaGreenTech.UserService.Application.Features.System.Queries.GetSystemModules;
using MediatR;
using OmniSync.Core.Infrastructure.Endpoints;

namespace AtaGreenTech.UserService.Api.Endpoints;

/// <summary>
/// <c>/api/v1/system</c>: dil / menü / modül kataloğu yönetimi (master DB).
/// </summary>
public sealed class SystemEndpoints : IEndpointModule
{
    public string Name => "system";

    public void MapEndpoints(RouteGroupBuilder group)
    {
        group.MapGet("/languages", async (ISender sender, CancellationToken cancellationToken) =>
                TypedResults.Ok(await sender.Send(new GetSystemLanguagesQuery(), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.SystemManage)
            .WithTags("System");

        group.MapGet("/modules", async (ISender sender, CancellationToken cancellationToken) =>
                TypedResults.Ok(await sender.Send(new GetSystemModulesQuery(), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.SystemManage)
            .WithTags("System");

        group.MapPut("/modules/{id:guid}", UpdateModuleAsync)
            .RequireAuthorization(AuthorizationPolicies.SystemManage)
            .WithTags("System");

        group.MapGet("/menus", async (ISender sender, CancellationToken cancellationToken) =>
                TypedResults.Ok(await sender.Send(new GetSystemMenusQuery(), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.SystemManage)
            .WithTags("System");

        group.MapPost("/menus", CreateMenuAsync)
            .RequireAuthorization(AuthorizationPolicies.SystemManage)
            .WithTags("System");

        group.MapGet("/menus/{id:guid}", async (Guid id, ISender sender, CancellationToken cancellationToken) =>
                TypedResults.Ok(await sender.Send(new GetSystemMenuByIdQuery(id), cancellationToken)))
            .RequireAuthorization(AuthorizationPolicies.SystemManage)
            .WithTags("System");

        group.MapPut("/menus/{id:guid}", UpdateMenuAsync)
            .RequireAuthorization(AuthorizationPolicies.SystemManage)
            .WithTags("System");

        group.MapPatch("/menus/{id:guid}", PatchMenuAsync)
            .RequireAuthorization(AuthorizationPolicies.SystemManage)
            .WithTags("System");
    }

    private static async Task<IResult> CreateMenuAsync(
        CreateSystemMenuRequest body,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var id = await sender.Send(
            new CreateSystemMenuCommand(
                body.Code,
                body.ModuleId,
                body.ParentId,
                body.RoutePath,
                body.IconKey,
                body.RequiredPermission,
                body.DisplayOrder,
                body.IsActive,
                body.Translations
            ),
            cancellationToken
        );
        return TypedResults.Created($"/api/v1/system/menus/{id}", new { id });
    }

    private static async Task<IResult> UpdateMenuAsync(
        Guid id,
        UpdateSystemMenuRequest body,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new UpdateSystemMenuCommand(
                id,
                body.Code,
                body.ModuleId,
                body.ParentId,
                body.RoutePath,
                body.IconKey,
                body.RequiredPermission,
                body.DisplayOrder,
                body.IsActive,
                body.Translations
            ),
            cancellationToken
        );
        return TypedResults.NoContent();
    }

    private static async Task<IResult> PatchMenuAsync(
        Guid id,
        PatchSystemMenuRequest body,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new PatchSystemMenuCommand(id, body.IsActive, body.DisplayOrder),
            cancellationToken
        );
        return TypedResults.NoContent();
    }

    private static async Task<IResult> UpdateModuleAsync(
        Guid id,
        UpdateSystemModuleRequest body,
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new UpdateSystemModuleCommand(
                id,
                body.IconKey,
                body.DisplayOrder,
                body.IsActive,
                body.Translations
            ),
            cancellationToken
        );
        return TypedResults.NoContent();
    }
}
