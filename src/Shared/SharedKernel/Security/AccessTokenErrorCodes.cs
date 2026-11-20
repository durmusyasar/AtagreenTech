namespace AtaGreenTech.SharedKernel.Security;

/// <summary>#70 Zero-Trust: access token / permission reddi için ortak hata kodları.</summary>
public static class AccessTokenErrorCodes
{
    public const string TokenRevoked = "TOKEN_REVOKED";
    public const string PermissionDenied = "PERMISSION_DENIED";
}
