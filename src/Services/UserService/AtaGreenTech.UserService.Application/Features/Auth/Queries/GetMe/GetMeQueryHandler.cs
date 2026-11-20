using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Tenants;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Auth.Queries.GetMe
{
    /// <summary>
    /// JWT <c>sub</c> + host tenant ile tenant DB’den güncel profil/rol/izin okur.
    /// TenantId body/header’dan alınmaz; yalnızca <see cref="ICurrentTenant"/> (#71 / zero-trust).
    /// </summary>
    internal sealed class GetMeQueryHandler(
        IUserDbContext db,
        ICurrentUser currentUser,
        ICurrentTenant currentTenant
    ) : IQueryHandler<GetMeQuery, GetMeResult>
    {
        public async Task<GetMeResult> Handle(GetMeQuery request, CancellationToken cancellationToken)
        {
            if (!currentUser.IsAuthenticated
                || string.IsNullOrWhiteSpace(currentUser.UserId)
                || !Guid.TryParse(currentUser.UserId, out var userId))
                return GetMeResult.Fail(AuthErrorCodes.InvalidCredentials);

            if (!currentTenant.IsAvailable || currentTenant.Id is null)
                return GetMeResult.Fail(AuthErrorCodes.TenantUnresolved, statusCode: 400);

            var user = await db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user is null)
                return GetMeResult.Fail(AuthErrorCodes.InvalidCredentials);

            if (!user.IsActive)
                return GetMeResult.Fail(AuthErrorCodes.AccountInactive, statusCode: 403);

            var roles = user.UserRoles
                .Where(ur => !ur.IsDeleted && ur.Role is { IsDeleted: false })
                .Select(ur => ur.Role.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(r => r, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var permissions = UserPermissionResolver.FromLoadedUser(user);

            return GetMeResult.Ok(
                new UserMeResponse(
                    user.Id,
                    user.Email,
                    user.Username,
                    user.FirstName,
                    user.LastName,
                    currentTenant.Id.Value.ToString(),
                    roles,
                    permissions,
                    user.IsMfaEnabled
                )
            );
        }
    }
}