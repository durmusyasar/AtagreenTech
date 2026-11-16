using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.VerifyMfa;

/// <summary>Kurulum TOTP kodunu doğrular ve MFA'yı etkinleştirir.</summary>
public sealed record VerifyMfaCommand(string Code) : ICommand<MessageResult>;
