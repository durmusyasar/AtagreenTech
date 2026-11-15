using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Tenants;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Auth.Queries.GetMe;

internal sealed class GetMeQueryHandler(
    IUserDbContext db,
    ICurrentUser currentUser,
    ICurrentTenant currentTenant
) : IQueryHandler<GetMeQuery, UserMeResponse?>
{
    public async Task<UserMeResponse?> Handle(GetMeQuery request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated
            || string.IsNullOrWhiteSpace(currentUser.UserId)
            || !Guid.TryParse(currentUser.UserId, out var userId))
            return null;

        if (!currentTenant.IsAvailable || currentTenant.Id is null)
            return null;

        var user = await db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null || !user.IsActive)
            return null;

        var roles = user.UserRoles
            .Where(ur => !ur.IsDeleted && ur.Role is { IsDeleted: false })
            .Select(ur => ur.Role.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var permissions = await UserPermissionResolver.ResolveAsync(db, user.Id, cancellationToken);

        return new UserMeResponse(
            user.Id,
            user.Email,
            user.Username,
            user.FirstName,
            user.LastName,
            currentTenant.Id.Value.ToString(),
            roles,
            permissions
        );
    }
}
