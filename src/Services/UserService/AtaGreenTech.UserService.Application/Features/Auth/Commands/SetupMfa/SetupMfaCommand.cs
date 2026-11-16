using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.SetupMfa;

/// <summary>Oturumdaki kullanıcı için TOTP secret + kurtarma kodları üretir (henüz etkinleştirmez).</summary>
public sealed record SetupMfaCommand : ICommand<MfaSetupResult>;
