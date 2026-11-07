using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Application.Interfaces;

/// <summary>
/// O anki işlemi başlatan isteğin kaynağını (istemci IP'si, izleme kimliği) stok hareketlerinin denetim izine verir.
/// </summary>
public interface IRequestOriginProvider
{
    MovementOrigin GetOrigin();
}
