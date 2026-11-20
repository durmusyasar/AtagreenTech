using AtaGreenTech.UserService.Application.Features.System.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Commands.CreateSystemMenu;

internal sealed class CreateSystemMenuCommandHandler(
    ISystemNavigationAdmin admin,
    INavigationMenuCache navigationMenuCache
) : ICommandHandler<CreateSystemMenuCommand, Guid>
{
    public async Task<Guid> Handle(CreateSystemMenuCommand request, CancellationToken cancellationToken)
    {
        var id = await admin.CreateMenuAsync(
            new CreateSystemMenuRequest(
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
        return id;
    }
}
