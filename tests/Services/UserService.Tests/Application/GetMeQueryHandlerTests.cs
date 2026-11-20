using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Features.Auth.Queries.GetMe;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using AtaGreenTech.UserService.Tests.Fakes;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class GetMeQueryHandlerTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111");
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-2222-2222-2222-222222222222");

    [Fact]
    public async Task Fails_with_invalid_credentials_when_unauthenticated()
    {
        var handler = CreateHandler(
            new FakeCurrentUser { IsAuthenticated = false },
            tenantId: TenantId,
            db: new ThrowingUserDbContext()
        );

        var result = await handler.Handle(new GetMeQuery(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AuthErrorCodes.InvalidCredentials, result.ErrorCode);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task Fails_with_tenant_unresolved_when_host_has_no_tenant()
    {
        var handler = CreateHandler(
            new FakeCurrentUser { IsAuthenticated = true, UserId = UserId.ToString() },
            tenantId: null,
            db: new ThrowingUserDbContext()
        );

        var result = await handler.Handle(new GetMeQuery(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AuthErrorCodes.TenantUnresolved, result.ErrorCode);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task Fails_with_invalid_credentials_when_user_id_not_guid()
    {
        var handler = CreateHandler(
            new FakeCurrentUser { IsAuthenticated = true, UserId = "not-a-guid" },
            tenantId: TenantId,
            db: new ThrowingUserDbContext()
        );

        var result = await handler.Handle(new GetMeQuery(), CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal(AuthErrorCodes.InvalidCredentials, result.ErrorCode);
    }

    private static GetMeQueryHandler CreateHandler(
        ICurrentUser currentUser,
        Guid? tenantId,
        IUserDbContext db
    ) =>
        new(
            db,
            currentUser,
            new FakeCurrentTenant
            {
                Id = tenantId,
                IsAvailable = tenantId is not null,
                Identifier = tenantId is null ? null : "atagreen",
            }
        );

    /// <summary>Erken dönüşlerde DB’ye hiç gidilmediğini doğrular.</summary>
    private sealed class ThrowingUserDbContext : IUserDbContext
    {
        public IQueryable<User> Users => throw new InvalidOperationException("Users should not be queried.");
        public IQueryable<Role> Roles => throw new InvalidOperationException();
        public IQueryable<UserRole> UserRoles => throw new InvalidOperationException();
        public IQueryable<Permission> Permissions => throw new InvalidOperationException();
        public IQueryable<RolePermission> RolePermissions => throw new InvalidOperationException();
        public IQueryable<RefreshToken> RefreshTokens => throw new InvalidOperationException();
        public IQueryable<AuditLog> AuditLogs => throw new InvalidOperationException();
    }
}
