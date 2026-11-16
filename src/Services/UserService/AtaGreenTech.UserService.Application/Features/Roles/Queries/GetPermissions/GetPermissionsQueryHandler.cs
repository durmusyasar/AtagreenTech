using AtaGreenTech.UserService.Application.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Roles.Queries.GetPermissions;

internal sealed class GetPermissionsQueryHandler(IUserDbContext db)
    : IQueryHandler<GetPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    public async Task<IReadOnlyList<PermissionDto>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken
    )
    {
        return await db.Permissions
            .OrderBy(p => p.Name)
            .Select(p => new PermissionDto(p.Id, p.Name, p.Description))
            .ToListAsync(cancellationToken);
    }
}
