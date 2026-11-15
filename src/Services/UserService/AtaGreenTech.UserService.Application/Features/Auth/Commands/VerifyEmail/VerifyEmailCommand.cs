using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyEmail;

public sealed record VerifyEmailCommand(string Token) : ICommand<MessageResult>;
