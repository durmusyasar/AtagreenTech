using AtaGreenTech.UserService.Application.DTOs;
using AtaGreenTech.UserService.Application.Features.Users;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Exceptions;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandHandler(
    IRepository<User> users,
    IRepository<UserRole> userRoles,
    IUserDbContext db
) : ICommandHandler<UpdateUserCommand, UserWriteResult>
{
    public async Task<UserWriteResult> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        user.UpdateProfile(request.FirstName, request.LastName, user.IsActive);

        IReadOnlyList<string> roleNames;
        if (request.Roles is not null)
        {
            var desired = await UserManagementHelpers.ResolveRolesAsync(db.Roles, request.Roles, cancellationToken);
            if (desired.Count == 0)
                return UserWriteResult.Fail("En az bir geçerli rol seçilmelidir.");

            var existing = await userRoles.GetAllByFilterAsync(ur => ur.UserId == user.Id, cancellationToken);
            var desiredIds = desired.Select(r => r.Id).ToHashSet();

            foreach (var assignment in existing.Where(ur => !desiredIds.Contains(ur.RoleId)))
                userRoles.Delete(assignment);

            var existingIds = existing.Select(ur => ur.RoleId).ToHashSet();
            foreach (var role in desired.Where(r => !existingIds.Contains(r.Id)))
                await userRoles.AddAsync(new UserRole(user.Id, role.Id), cancellationToken);

            roleNames = desired
                .Select(r => r.Name)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        else
        {
            roleNames = await db.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync(cancellationToken);
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
