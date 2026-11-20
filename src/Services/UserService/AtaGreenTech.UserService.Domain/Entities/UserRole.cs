using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Domain.Entities
{
    /// <summary>
    /// Kullanıcı–rol ataması. Soft-delete ile geri alınabilir atamalar için kendi kimliği vardır;
    /// silinmemiş kayıtlar arasında (UserId, RoleId) benzersizdir.
    /// Boş kimliklerde <see cref="DomainErrorCodes.ValidationRequired"/> fırlatılır (#71).
    /// </summary>
    public class UserRole : BaseEntity
    {
        protected UserRole()
        {
            User = null!;
            Role = null!;
        }

        public UserRole(Guid userId, Guid roleId)
            : this()
        {
            if (userId == Guid.Empty)
                throw new ArgumentException(DomainErrorCodes.ValidationRequired, nameof(userId));
            if (roleId == Guid.Empty)
                throw new ArgumentException(DomainErrorCodes.ValidationRequired, nameof(roleId));

            UserId = userId;
            RoleId = roleId;
        }

        public Guid UserId { get; private set; }

        public User User { get; private set; }

        public Guid RoleId { get; private set; }

        public Role Role { get; private set; }
    }
}