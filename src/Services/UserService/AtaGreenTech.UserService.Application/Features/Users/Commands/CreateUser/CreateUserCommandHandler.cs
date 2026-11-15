using AtaGreenTech.UserService.Application.DTOs;
using AtaGreenTech.UserService.Application.Features.Users;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.CreateUser;

internal sealed class CreateUserCommandHandler(
    IRepository<User> users,
    IRepository<UserRole> userRoles,
    IUserDbContext db,
    IPasswordHasher passwordHasher
) : ICommandHandler<CreateUserCommand, UserWriteResult>
{
    public async Task<UserWriteResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var email = User.NormalizeEmail(request.Email);
        if (await users.ExistsAsync(u => u.Email == email, cancellationToken))
            return UserWriteResult.Fail("Bu e-posta adresi zaten kullanımda.");

        var username = UserManagementHelpers.ResolveUsername(email);
        if (await users.ExistsAsync(u => u.Username == username, cancellationToken))
            return UserWriteResult.Fail("Bu kullanıcı adı zaten kullanılıyor.");

        var roles = await UserManagementHelpers.ResolveRolesAsync(db.Roles, request.Roles, cancellationToken);
        if (roles.Count == 0)
            return UserWriteResult.Fail("En az bir geçerli rol seçilmelidir.");

        var user = new User(
            email,
            username,
            passwordHasher.Hash(request.Password),
            request.FirstName,
            request.LastName
        );
        user.MarkEmailVerified();
        if (!request.IsActive)
            user.Deactivate();

        await users.AddAsync(user, cancellationToken);
        foreach (var role in roles)
            await userRoles.AddAsync(new UserRole(user.Id, role.Id), cancellationToken);

        // Roller henüz navigation'da yok; DTO'yu atanan isimlerle kur.
        var createdAt = user.CreatedAt == default ? DateTime.UtcNow : user.CreatedAt;
        var dto = new UserDto(
            user.Id,
            user.Email,
            user.Username,
            user.FirstName,
            user.LastName,
            user.IsActive,
            user.IsEmailVerified,
            createdAt,
            roles.Select(r => r.Name).OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToArray()
        );

        return UserWriteResult.Ok(dto);
    }
}
