namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>MFA kurulum yanıtı: shared key, QR URI ve tek seferlik kurtarma kodları.</summary>
public sealed record MfaSetupResponse(
    string SharedKey,
    string QrCodeUri,
    IReadOnlyList<string> RecoveryCodes
);
