using AtaGreenTech.UserService.Application.Features.Tenants.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Tenants.Commands.CreateTenant;

internal sealed class CreateTenantCommandHandler(
    ITenantManagementAdmin admin,
    ICurrentUser currentUser
) : ICommandHandler<CreateTenantCommand, CreateTenantResult>
{
    public Task<CreateTenantResult> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        Guid? actorUserId = null;
        if (currentUser.IsAuthenticated
            && !string.IsNullOrWhiteSpace(currentUser.UserId)
            && Guid.TryParse(currentUser.UserId, out var userId))
        {
            actorUserId = userId;
        }

        return admin.CreateTenantAsync(
            new CreateTenantRequest(
                request.Name,
                request.Slug,
                request.Modules,
                request.AdminEmail,
                request.AdminFirstName,
                request.AdminLastName,
                request.AdminPassword
            ),
            actorUserId,
            cancellationToken
        );
    }
}
