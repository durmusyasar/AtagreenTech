using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AtaGreenTech.UserService.Application.Features.Auth
{
    /// <summary>
    /// Kullanıcının rollerinden birleşik izin adlarını üretir (JWT <c>permission</c> claim / menü filtresi).
    /// Soft-delete satırları açıkça dışlanır; global query filter’a ek savunma.
    /// </summary>
    internal static class UserPermissionResolver
    {
        /// <summary>
        /// Tenant DB üzerinden kullanıcının aktif rol–izin birleşimini okur; sıralı ve tekilleştirilmiş liste döner.
        /// </summary>
        public static async Task<IReadOnlyList<string>> ResolveAsync(
            IUserDbContext db,
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            var names = await db.UserRoles
                .Where(ur =>
                    ur.UserId == userId
                    && !ur.IsDeleted
                    && !ur.Role.IsDeleted
                )
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => !rp.IsDeleted && !rp.Permission.IsDeleted)
                .Select(rp => rp.Permission.Name)
                .ToListAsync(cancellationToken);

            return Normalize(names);
        }

        /// <summary>
        /// Önceden yüklenmiş <see cref="User"/> grafiğinden (Include) aynı birleşik izin listesini üretir.
        /// </summary>
        public static IReadOnlyList<string> FromLoadedUser(User user) =>
            Normalize(
                user.UserRoles
                    .Where(ur => !ur.IsDeleted && ur.Role is { IsDeleted: false })
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Where(rp => !rp.IsDeleted && rp.Permission is { IsDeleted: false })
                    .Select(rp => rp.Permission.Name)
            );

        /// <summary>Büyük/küçük harf duyarsız tekilleştirme + sıralama (JWT claim kararlılığı).</summary>
        private static IReadOnlyList<string> Normalize(IEnumerable<string> names) =>
            names
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToArray();
    }
}