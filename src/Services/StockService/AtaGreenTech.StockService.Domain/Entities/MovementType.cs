namespace AtaGreenTech.StockService.Domain.Entities;

public enum MovementType
{
    /// <summary>
    /// Depoya giriş (satın alma, üretim girişi, iade).
    /// </summary>
    Inbound = 1,

    /// <summary>
    /// Depodan çıkış (satış, fire, sevkiyat).
    /// </summary>
    Outbound = 2,

    /// <summary>
    /// İki depo arasında transfer.
    /// </summary>
    Transfer = 3,
}
