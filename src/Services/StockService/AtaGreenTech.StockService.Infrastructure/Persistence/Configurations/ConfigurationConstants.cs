using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Infrastructure.Persistence.Configurations;

internal static class ConfigurationConstants
{
    /// <summary>
    /// Benzersiz index'leri yalnızca silinmemiş kayıtlara uygulayan PostgreSQL partial index filtresi; böylece silinen
    /// bir kaydın SKU'su veya kodu yeniden kullanılabilir. Sütun adları EF'in ürettiği gibi tırnaklı PascalCase'dir.
    /// </summary>
    public const string NotDeletedFilter = $"\"{nameof(BaseEntity.IsDeleted)}\" = false";
}
