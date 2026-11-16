using AtaGreenTech.UserService.Application.DTOs;
using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Roles.Queries.GetRoles;

internal sealed class GetRolesQueryHandler(IUserDbContext db)
    : IQueryHandler<GetRolesQuery, IReadOnlyList<RoleDetailDto>>
{
    public async Task<IReadOnlyList<RoleDetailDto>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken
    )
    {
        var roles = await db.Roles
            .OrderBy(r => r.Name)
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
            })
            .ToListAsync(cancellationToken);

        var roleIds = roles.Select(r => r.Id).ToArray();
        var grants = await (
            from rp in db.RolePermissions
            join p in db.Permissions on rp.PermissionId equals p.Id
            where roleIds.Contains(rp.RoleId)
            select new { rp.RoleId, PermissionName = p.Name }
        ).ToListAsync(cancellationToken);

        var byRole = grants
            .GroupBy(g => g.RoleId)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<string>)g
                    .Select(x => x.PermissionName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            );

        return roles
            .Select(r => new RoleDetailDto(
                r.Id,
                r.Name,
                r.Description,
                byRole.TryGetValue(r.Id, out var perms) ? perms : Array.Empty<string>()
            ))
            .ToArray();
    }
}
