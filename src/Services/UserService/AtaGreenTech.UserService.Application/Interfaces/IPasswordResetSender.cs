namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>Şifre sıfırlama linki gönderimi (SMTP/SendGrid veya geliştirmede log).</summary>
public interface IPasswordResetSender
{
    Task SendAsync(string email, string resetUrl, CancellationToken cancellationToken = default);
}
