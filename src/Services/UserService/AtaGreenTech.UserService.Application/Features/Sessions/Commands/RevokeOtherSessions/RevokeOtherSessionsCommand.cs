using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Sessions.Commands.RevokeOtherSessions;

/// <param name="CurrentRefreshToken">Korunacak aktif oturumun düz metin refresh token'ı.</param>
public sealed record RevokeOtherSessionsCommand(string CurrentRefreshToken) : ICommand<MessageResult>;
