namespace AtaGreenTech.UserService.Api.Contracts
{
    /// <summary>API hata yanıtı için tutarlı gövde.</summary>
    /// <param name="MfaRequired">Login ikinci adımı gerektiğinde <see langword="true"/>.</param>
    public sealed record ErrorResponse(string Code, string Message, bool MfaRequired = false);
}
