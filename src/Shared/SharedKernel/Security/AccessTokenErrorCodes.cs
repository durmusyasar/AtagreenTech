using AtaGreenTech.SharedKernel.Localization;

namespace AtaGreenTech.SharedKernel.Security;

/// <summary>#70 Zero-Trust: access token / permission reddi için ortak hata kodları (#71 SharedResource anahtarları).</summary>
public static class AccessTokenErrorCodes
{
    public const string TokenRevoked = ErrorCodes.TokenRevoked;
    public const string PermissionDenied = ErrorCodes.PermissionDenied;
}
