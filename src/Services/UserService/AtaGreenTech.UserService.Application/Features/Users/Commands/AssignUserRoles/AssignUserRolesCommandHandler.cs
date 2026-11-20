using AtaGreenTech.UserService.Application.DTOs;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Users;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.AssignUserRoles;

internal sealed class AssignUserRolesCommandHandler(
    IRepository<User> users,
    IRepository<UserRole> userRoles,
    IUserDbContext db,
    IIdentityAuditLogger auditLogger,
    ICurrentUser currentUser,
    AccessTokenLifecycle accessTokenLifecycle
) : ICommandHandler<AssignUserRolesCommand, UserWriteResult>
{
    public async Task<UserWriteResult> Handle(
        AssignUserRolesCommand request,
        CancellationToken cancellationToken
    )
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        var desired = await UserManagementHelpers.ResolveRolesAsync(db.Roles, request.Roles, cancellationToken);
        if (desired.Count == 0)
            return UserWriteResult.Fail("En az bir geçerli rol seçilmelidir.");

        var existing = await userRoles.GetAllByFilterAsync(ur => ur.UserId == user.Id, cancellationToken);
        var desiredIds = desired.Select(r => r.Id).ToHashSet();
        var existingIds = existing.Select(ur => ur.RoleId).ToHashSet();
        var rolesChanged = !desiredIds.SetEquals(existingIds);

        foreach (var assignment in existing.Where(ur => !desiredIds.Contains(ur.RoleId)))
            userRoles.Delete(assignment);

        foreach (var role in desired.Where(r => !existingIds.Contains(r.Id)))
            await userRoles.AddAsync(new UserRole(user.Id, role.Id), cancellationToken);

        var roleNames = desired
            .Select(r => r.Name)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (rolesChanged)
        {
            user.InvalidateCredentialStamp();
            await accessTokenLifecycle.PublishStampAsync(user.Id, user.SecurityStamp, cancellationToken);

            Guid? actorId = Guid.TryParse(currentUser.UserId, out var id) ? id : null;
            var actorEmail = string.IsNullOrWhiteSpace(currentUser.Email) ? "unknown" : currentUser.Email;
            await auditLogger.LogAsync(
                AuditLog.Actions.RoleChanged,
                actorEmail,
                actorId,
                request.ClientIp,
                $"{user.Email} kullanıcısının rolleri güncellendi: {string.Join(", ", roleNames)}.",
                cancellationToken
            );
        }

        return UserWriteResult.Ok(
            new UserDto(
                user.Id,
                user.Email,
                user.Username,
                user.FirstName,
                user.LastName,
                user.IsActive,
                user.IsEmailVerified,
                user.CreatedAt,
                roleNames
            )
        );
    }
}
