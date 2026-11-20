namespace AtaGreenTech.UserService.Application.Interfaces
{
    /// <summary>
    /// Kimlik olaylarını tenant DB'ye yazar. Komut transaction'ı commit edilince kalıcı olur;
    /// ayrı <c>SaveChanges</c> çağrılmaz (başarısız login + lockout ile aynı transaction).
    /// </summary>
    public interface IIdentityAuditLogger
    {
        /// <summary>
        /// Append-only denetim satırı ekler (<see cref="AtaGreenTech.UserService.Domain.Entities.AuditLog"/>).
        /// Kalıcılık çağıranın UoW commit'ine bağlıdır.
        /// </summary>
        /// <param name="action"><see cref="AtaGreenTech.UserService.Domain.Entities.AuditLog.Actions"/> sabitlerinden biri (normalize edilir).</param>
        /// <param name="email">Olayla ilişkili e-posta (normalize edilir; zorunlu).</param>
        /// <param name="userId">Biliniyorsa kullanıcı kimliği.</param>
        /// <param name="ipAddress">İstemci IP (opsiyonel; uzunsa kesilir).</param>
        /// <param name="details">
        /// Audit ayrıntısı: çağıran <see cref="AtaGreenTech.SharedKernel.Localization.ErrorCodes.Audit"/>
        /// + <c>IErrorLocalizer</c> ile üretir (#71); sabit kullanıcı/audit metni yazılmaz.
        /// </param>
        /// <param name="cancellationToken">İptal jetonu.</param>
        Task LogAsync(
            string action,
            string email,
            Guid? userId = null,
            string? ipAddress = null,
            string? details = null,
            CancellationToken cancellationToken = default
        );
    }
}