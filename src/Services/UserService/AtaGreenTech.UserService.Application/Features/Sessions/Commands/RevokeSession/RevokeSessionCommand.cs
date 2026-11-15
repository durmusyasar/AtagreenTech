using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Sessions.Commands.RevokeSession;

public sealed record RevokeSessionCommand(Guid SessionId) : ICommand<MessageResult>;
