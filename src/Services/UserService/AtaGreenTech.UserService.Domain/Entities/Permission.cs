using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Domain.Entities
{
    /// <summary>
    /// Tenant RBAC izin kaydı (ör. <c>users.read</c>). Soft-delete destekler; silinmemiş kayıtlar arasında Name benzersizdir.
    /// Invariant ihlallerinde <see cref="DomainErrorCodes"/> taşınır (#71).
    /// </summary>
    public class Permission : BaseEntity
    {
        public const int NameMaxLength = 128;
        public const int DescriptionMaxLength = 256;

        private readonly List<RolePermission> _rolePermissions = [];

        protected Permission()
        {
            Name = null!;
            Description = null!;
        }

        public Permission(string name, string? description = null)
            : this()
        {
            SetDetails(name, description);
        }

        /// <summary>Normalize edilmiş izin anahtarı (ör. users.read).</summary>
        public string Name { get; private set; }

        public string Description { get; private set; }

        public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

        public void Update(string name, string? description) => SetDetails(name, description);

        public static string NormalizeName(string name) => name.Trim().ToLowerInvariant();

        private void SetDetails(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(DomainErrorCodes.ValidationRequired, nameof(name));

            var normalized = NormalizeName(name);
            if (normalized.Length > NameMaxLength)
                throw new ArgumentException(DomainErrorCodes.ValidationMaxLength, nameof(name));

            var trimmedDescription = string.IsNullOrWhiteSpace(description)
                ? string.Empty
                : description.Trim();
            if (trimmedDescription.Length > DescriptionMaxLength)
                throw new ArgumentException(DomainErrorCodes.ValidationMaxLength, nameof(description));

            Name = normalized;
            Description = trimmedDescription;
        }
    }
}