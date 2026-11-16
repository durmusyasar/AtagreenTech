using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.DisableMfa;

/// <summary>Mevcut şifre ile MFA'yı kapatır; secret ve kurtarma kodlarını temizler.</summary>
public sealed record DisableMfaCommand(string Password) : ICommand<MessageResult>;
