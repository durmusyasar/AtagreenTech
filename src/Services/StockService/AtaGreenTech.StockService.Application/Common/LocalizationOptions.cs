namespace AtaGreenTech.StockService.Application.Common;

/// <summary>
/// <c>Localization</c> konfigürasyon bölümü.
/// </summary>
public sealed class LocalizationOptions
{
    public const string SectionName = "Localization";

    /// <summary>
    /// Sistemin varsayılan (aktif) dili. Çok dilli kayıtlarda bu dilde çeviri zorunludur; istenen dilde çeviri
    /// bulunmadığında da bu dil gösterilir.
    /// </summary>
    public string DefaultLanguage { get; set; } = "tr";
}
