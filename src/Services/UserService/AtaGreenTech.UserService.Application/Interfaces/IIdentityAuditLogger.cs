namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>
/// Kimlik olaylarını tenant DB'ye yazar. Komut transaction'ı commit edilince kalıcı olur;
/// ayrı <c>SaveChanges</c> çağrılmaz (başarısız login + lockout ile aynı transaction).
/// </summary>
public interface IIdentityAuditLogger
{
    Task LogAsync(
        string action,
        string email,
        Guid? userId = null,
        string? ipAddress = null,
        string? details = null,
        CancellationToken cancellationToken = default
    );
}
