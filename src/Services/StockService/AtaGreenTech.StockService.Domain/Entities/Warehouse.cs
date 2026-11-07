using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.StockService.Domain.Entities;

/// <summary>
/// Stoğun tutulduğu fiziksel veya sanal depo (ana depo, şube deposu, üretim deposu vb.).
/// </summary>
public class Warehouse : BaseEntity
{
    public const int NameMaxLength = 150;
    public const int CodeMaxLength = 50;
    public const int AddressMaxLength = 500;

    protected Warehouse()
    {
        Name = null!;
        Code = null!;
    }

    public Warehouse(string name, string code, string? address)
        : this()
    {
        SetDetails(name, code, address);
    }

    public string Name { get; private set; }

    /// <summary>
    /// Silinmemiş depolar arasında benzersiz kısa kod (ör. MTR-01); büyük harfe çevrilerek saklanır.
    /// </summary>
    public string Code { get; private set; }

    public string? Address { get; private set; }

    public bool IsActive { get; private set; } = true;

    public void Update(string name, string code, string? address, bool isActive)
    {
        SetDetails(name, code, address);
        IsActive = isActive;
    }

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private void SetDetails(string name, string code, string? address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        Name = name.Trim();
        Code = NormalizeCode(code);
        Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
    }
}
