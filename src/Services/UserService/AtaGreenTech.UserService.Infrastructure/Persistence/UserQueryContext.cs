using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using OmniSync.Core.Infrastructure.Persistence;

namespace AtaGreenTech.UserService.Infrastructure.Persistence;

/// <summary>
/// <see cref="IUserDbContext"/>'i isteğin tenant'ına bağlı <see cref="TenantDbContext"/> üzerinden sağlar.
/// Ayrı bir DbContext türetilmez.
/// </summary>
internal sealed class UserQueryContext(TenantDbContext context) : IUserDbContext
{
    public IQueryable<User> Users => context.Set<User>().AsNoTracking();

    public IQueryable<Role> Roles => context.Set<Role>().AsNoTracking();

    public IQueryable<UserRole> UserRoles => context.Set<UserRole>().AsNoTracking();

    public IQueryable<RefreshToken> RefreshTokens => context.Set<RefreshToken>().AsNoTracking();
}
