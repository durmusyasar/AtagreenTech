using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Tenants.Commands.UpdateTenantModules;

internal sealed class UpdateTenantModulesCommandHandler(
    ITenantManagementAdmin admin,
    INavigationMenuCache navigationMenuCache,
    ICurrentUser currentUser
) : ICommandHandler<UpdateTenantModulesCommand>
{
    public async Task Handle(UpdateTenantModulesCommand request, CancellationToken cancellationToken)
    {
        Guid? actorUserId = null;
        if (currentUser.IsAuthenticated
            && !string.IsNullOrWhiteSpace(currentUser.UserId)
            && Guid.TryParse(currentUser.UserId, out var userId))
        {
            actorUserId = userId;
        }

        await admin.UpdateTenantModulesAsync(
            request.TenantId,
            request.Modules,
            actorUserId,
            cancellationToken
        );
        navigationMenuCache.InvalidateTenant(request.TenantId);
    }
}
