using AtaGreenTech.UserService.Application.Features.System.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Commands.UpdateSystemModule;

internal sealed class UpdateSystemModuleCommandHandler(
    ISystemNavigationAdmin admin,
    INavigationMenuCache navigationMenuCache
) : ICommandHandler<UpdateSystemModuleCommand>
{
    public async Task Handle(UpdateSystemModuleCommand request, CancellationToken cancellationToken)
    {
        await admin.UpdateModuleAsync(
            request.Id,
            new UpdateSystemModuleRequest(
                request.IconKey,
                request.DisplayOrder,
                request.IsActive,
                request.Translations
            ),
            cancellationToken
        );
        navigationMenuCache.InvalidateAll();
    }
}
