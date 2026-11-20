using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Domain.Entities
{
    /// <summary>
    /// Tenant içi yetki rolü (ör. Admin, Manager, Viewer). Roller tenant DB'sinde tutulur; aynı isimler
    /// her tenant'ta bağımsız kayıtlar olarak oluşabilir.
    /// Invariant ihlallerinde <see cref="DomainErrorCodes"/> taşınır (#71).
    /// </summary>
    public class Role : BaseEntity
    {
        public const int NameMaxLength = 64;
        public const int DescriptionMaxLength = 256;

        public static class Names
        {
            public const string Admin = "Admin";
            public const string Manager = "Manager";
            public const string Viewer = "Viewer";
        }

        private readonly List<UserRole> _userRoles = [];
        private readonly List<RolePermission> _rolePermissions = [];

        protected Role()
        {
            Name = null!;
            Description = null!;
        }

        public Role(string name, string? description = null)
            : this()
        {
            SetDetails(name, description);
        }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

        public void Update(string name, string? description)
        {
            SetDetails(name, description);
        }

        public static string NormalizeName(string name) => name.Trim();

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