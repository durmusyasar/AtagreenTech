namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>
/// E-posta doğrulama linki gönderimi. Üretimde SMTP/SendGrid; geliştirmede log ile simüle edilebilir.
/// </summary>
public interface IEmailVerificationSender
{
    Task SendAsync(string email, string verificationUrl, CancellationToken cancellationToken = default);
}
