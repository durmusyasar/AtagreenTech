namespace AtaGreenTech.UserService.Application.Interfaces;

/// <summary>Kullanıcı davet linki gönderimi (SMTP/SendGrid veya geliştirmede log).</summary>
public interface IUserInvitationSender
{
    Task SendAsync(string email, string inviteUrl, CancellationToken cancellationToken = default);
}
