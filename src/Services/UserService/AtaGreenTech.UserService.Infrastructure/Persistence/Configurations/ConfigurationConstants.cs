using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Infrastructure.Persistence.Configurations;

internal static class ConfigurationConstants
{
    /// <summary>
    /// Benzersiz index'leri yalnızca silinmemiş kayıtlara uygulayan PostgreSQL partial index filtresi.
    /// Sütun adları EF'in ürettiği gibi tırnaklı PascalCase'dir.
    /// </summary>
    public const string NotDeletedFilter = $"\"{nameof(BaseEntity.IsDeleted)}\" = false";
}
