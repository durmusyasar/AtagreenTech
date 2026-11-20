using AtaGreenTech.UserService.Application.Features.System.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.System.Commands.PatchSystemMenu;

internal sealed class PatchSystemMenuCommandHandler(
    ISystemNavigationAdmin admin,
    INavigationMenuCache navigationMenuCache
) : ICommandHandler<PatchSystemMenuCommand>
{
    public async Task Handle(PatchSystemMenuCommand request, CancellationToken cancellationToken)
    {
        await admin.PatchMenuAsync(
            request.Id,
            new PatchSystemMenuRequest(request.IsActive, request.DisplayOrder),
            cancellationToken
        );
        navigationMenuCache.InvalidateAll();
    }
}
