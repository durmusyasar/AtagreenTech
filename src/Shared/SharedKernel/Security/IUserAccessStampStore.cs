namespace AtaGreenTech.SharedKernel.Security;

/// <summary>
/// Kullanıcı <c>SecurityStamp</c> yayınları. StockService tenant User tablosuna bakmadan sst doğrular (#70).
/// </summary>
public interface IUserAccessStampStore
{
    Task PublishAsync(
        Guid tenantId,
        Guid userId,
        string securityStamp,
        CancellationToken cancellationToken = default
    );

    Task<string?> GetAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}
