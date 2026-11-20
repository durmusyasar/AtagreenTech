using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.AcceptInvite;

/// <summary>
/// Davet jetonunu tüketir; parola + profil yazar, hesabı aktifleştirir ve e-postayı doğrulanmış sayar.
/// Geçersiz/süresi dolmuş jeton → <c>AUTH_INVITE_TOKEN_INVALID</c>.
/// </summary>
public sealed record AcceptInviteCommand(string Token, string Password, string FirstName, string LastName)
    : ICommand<MessageResult>;
