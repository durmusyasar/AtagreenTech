using AtaGreenTech.UserService.Application.Features.System.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Commands.UpdateSystemMenu;

internal sealed class UpdateSystemMenuCommandHandler(
    ISystemNavigationAdmin admin,
    INavigationMenuCache navigationMenuCache
) : ICommandHandler<UpdateSystemMenuCommand>
{
    public async Task Handle(UpdateSystemMenuCommand request, CancellationToken cancellationToken)
    {
        await admin.UpdateMenuAsync(
            request.Id,
            new UpdateSystemMenuRequest(
                request.Code,
                request.ModuleId,
                request.ParentId,
                request.RoutePath,
                request.IconKey,
                request.RequiredPermission,
                request.DisplayOrder,
                request.IsActive,
                request.Translations
            ),
            cancellationToken
        );
        navigationMenuCache.InvalidateAll();
    }
}
