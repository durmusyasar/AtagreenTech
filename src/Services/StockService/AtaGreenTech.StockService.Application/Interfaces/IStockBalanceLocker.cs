using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Application.Interfaces;

/// <summary>
/// Stok bakiyelerini değiştirmeden önce satır düzeyinde kilitler. Kilit command'ın transaction'ı bitene kadar
/// tutulur; aynı bakiyeye gelen eşzamanlı hareketler sıraya girer ve her biri güncel miktarı görür.
/// </summary>
public interface IStockBalanceLocker
{
    /// <summary>
    /// Ürünün verilen depolardaki bakiyelerini kilitleyip değişiklikleri kaydedilecek şekilde döndürür. Bakiyesi
    /// olmayan depo için sıfır miktarlı kayıt açılır. Kilitler, karşılıklı beklemeyi (deadlock) önlemek için depo
    /// kimliğine göre sabit bir sırayla alınır.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, StockBalance>> AcquireAsync(
        Guid productId,
        IReadOnlyCollection<Guid> warehouseIds,
        CancellationToken cancellationToken
    );
}
