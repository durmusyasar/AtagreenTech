using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Users.Commands.UpdateUser;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;
using Microsoft.Extensions.Options;
using OmniSync.Core.Domain.Exceptions;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class UpdateUserCommandHandlerTests
{
    private static readonly Guid UserId = Guid.Parse("eeeeeeee-5555-5555-5555-555555555555");
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");

    [Fact]
    public async Task Throws_when_user_missing()
    {
        var handler = CreateHandler(out _);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(
                new UpdateUserCommand(UserId, "Ada", "Lovelace", Roles: null),
                CancellationToken.None
            )
        );
    }

    [Fact]
    public async Task Fails_when_roles_list_empty()
    {
        var handler = CreateHandler(out var users);
        var user = new User("a@b.com", "admin", "hash", "A", "B");
        SetUserId(user, UserId);
        await users.AddAsync(user, CancellationToken.None);

        // Boş isim listesi ResolveRolesAsync içinde EF’ye gitmeden [] döner → USER_ROLE_REQUIRED.
        var result = await handler.Handle(
            new UpdateUserCommand(UserId, "Ada", "Lovelace", Roles: []),
            CancellationToken.None
        );

        Assert.False(result.Succeeded);
        Assert.Equal(ErrorCodes.UserRoleRequired, result.ErrorCode);
    }

    private static UpdateUserCommandHandler CreateHandler(out InMemoryRepository<User> users)
    {
        users = new InMemoryRepository<User>();
        var stamps = new FakeUserAccessStampStore();
        var tenant = new FakeCurrentTenant
        {
            Id = TenantId,
            IsAvailable = true,
            Identifier = "atagreen",
        };
        var lifecycle = new AccessTokenLifecycle(
            stamps,
            new FakeAccessTokenRevocationStore(),
            tenant,
            Options.Create(new AuthOptions { AccessTokenMinutes = 30 })
        );

        return new UpdateUserCommandHandler(
            users,
            new InMemoryRepository<UserRole>(),
            new EmptyRolesDbContext(),
            new FakeIdentityAuditLogger(),
            new FakeCurrentUser { IsAuthenticated = true, UserId = UserId.ToString(), Email = "actor@b.com" },
            lifecycle,
            TestLocalizers.Passthrough
        );
    }

    private static void SetUserId(User user, Guid id) =>
        typeof(User).BaseType!.GetProperty("Id")!.SetValue(user, id);

    /// <summary>
    /// <see cref="UserManagementHelpers.ResolveRolesAsync"/> boş isimde EF’ye gitmeden döner;
    /// <c>Roles</c> property erişimi yine de gerekir.
    /// </summary>
    private sealed class EmptyRolesDbContext : IUserDbContext
    {
        public IQueryable<User> Users => Array.Empty<User>().AsQueryable();
        public IQueryable<Role> Roles => Array.Empty<Role>().AsQueryable();
        public IQueryable<UserRole> UserRoles => Array.Empty<UserRole>().AsQueryable();
        public IQueryable<Permission> Permissions => Array.Empty<Permission>().AsQueryable();
        public IQueryable<RolePermission> RolePermissions => Array.Empty<RolePermission>().AsQueryable();
        public IQueryable<RefreshToken> RefreshTokens => Array.Empty<RefreshToken>().AsQueryable();
        public IQueryable<AuditLog> AuditLogs => Array.Empty<AuditLog>().AsQueryable();
    }

    private sealed class FakeIdentityAuditLogger : IIdentityAuditLogger
    {
        public Task LogAsync(
            string action,
            string email,
            Guid? userId = null,
            string? ipAddress = null,
            string? details = null,
            CancellationToken cancellationToken = default
        ) =>
            Task.CompletedTask;
    }
}
