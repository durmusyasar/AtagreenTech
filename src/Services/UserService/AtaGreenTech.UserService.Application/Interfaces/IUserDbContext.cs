using AtaGreenTech.UserService.Domain.Entities;

namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>
/// Query handler'larının okuma modeli: isteğin tenant veritabanındaki identity tabloları, değişiklik takibi
/// olmadan (<c>AsNoTracking</c>) ve soft delete filtresi uygulanmış olarak. Yazma işlemleri
/// <c>IRepository&lt;T&gt;</c> üzerinden yapılır.
/// </summary>
public interface IUserDbContext
{
    IQueryable<User> Users { get; }

    IQueryable<Role> Roles { get; }

    IQueryable<UserRole> UserRoles { get; }

    IQueryable<RefreshToken> RefreshTokens { get; }
}
