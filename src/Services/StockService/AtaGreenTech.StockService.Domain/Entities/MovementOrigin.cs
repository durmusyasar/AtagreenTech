namespace AtaGreenTech.StockService.Domain.Entities;

/// <summary>
/// Stok hareketini başlatan isteğin kaynağı (istemci IP'si ve izleme kimliği).
/// </summary>
public sealed record MovementOrigin(string? ClientIpAddress, string? CorrelationId)
{
    public static readonly MovementOrigin None = new(null, null);
}
