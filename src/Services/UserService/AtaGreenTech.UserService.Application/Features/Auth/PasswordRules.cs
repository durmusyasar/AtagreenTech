using System.Text.RegularExpressions;
using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>
/// Güçlü şifre kuralları (FE <c>passwordPolicy.ts</c> ile hizalı): min 8 / max 128,
/// ASCII büyük/küçük harf, rakam ve özel karakter. Başarısızlıktan <see cref="ErrorCodes"/> döner.
/// </summary>
public static partial class PasswordRules
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    /// <summary>
    /// Şifreyi politika kurallarına göre doğrular; kullanıcı metni üretmez.
    /// </summary>
    /// <param name="password">Doğrulanacak şifre; null/boş min-length hatasıdır.</param>
    /// <returns>
    /// <c>IsValid</c>; başarısızsa <c>ErrorCode</c> (<see cref="ErrorCodes"/>) ve format <c>Args</c>.
    /// Başarıda <c>ErrorCode</c> boş string, <c>Args</c> boş dizidir.
    /// </returns>
    public static (bool IsValid, string ErrorCode, object[] Args) Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinLength)
            return (false, ErrorCodes.ValidationPasswordMinLength, [MinLength]);

        if (password.Length > MaxLength)
            return (false, ErrorCodes.ValidationPasswordMaxLength, [MaxLength]);

        // ASCII: FE checkPasswordStrength ile aynı sözleşme (Unicode harf/rakam kabul edilmez).
        if (!UpperRegex().IsMatch(password))
            return (false, ErrorCodes.ValidationPasswordUpper, []);

        if (!LowerRegex().IsMatch(password))
            return (false, ErrorCodes.ValidationPasswordLower, []);

        if (!DigitRegex().IsMatch(password))
            return (false, ErrorCodes.ValidationPasswordDigit, []);

        if (!SpecialCharRegex().IsMatch(password))
            return (false, ErrorCodes.ValidationPasswordSpecial, []);

        return (true, string.Empty, []);
    }

    /// <summary>
    /// FluentValidation kuralı: <see cref="Validate"/> sonucunu <see cref="IErrorLocalizer"/> ile alan hatasına yazar.
    /// </summary>
    public static IRuleBuilderOptionsConditions<T, string> ApplyStrongPassword<T>(
        this IRuleBuilder<T, string> rule,
        IErrorLocalizer localizer
    ) =>
        rule.Custom(
            (password, context) =>
            {
                var (isValid, errorCode, args) = Validate(password);
                if (!isValid)
                    context.AddFailure(localizer.Localize(errorCode, args));
            }
        );

    [GeneratedRegex(@"[A-Z]")]
    private static partial Regex UpperRegex();

    [GeneratedRegex(@"[a-z]")]
    private static partial Regex LowerRegex();

    [GeneratedRegex(@"\d")]
    private static partial Regex DigitRegex();

    [GeneratedRegex(@"[^A-Za-z0-9]")]
    private static partial Regex SpecialCharRegex();
}
