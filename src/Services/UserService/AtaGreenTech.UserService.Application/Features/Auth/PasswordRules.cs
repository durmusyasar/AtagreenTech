using System.Text.RegularExpressions;
using AtaGreenTech.SharedKernel.Localization;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>
/// Güçlü şifre kuralları: min 8 karakter, büyük/küçük harf, rakam ve özel karakter.
/// </summary>
public static partial class PasswordRules
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public static (bool IsValid, string ErrorCode, object[] Args) Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinLength)
            return (false, ErrorCodes.ValidationPasswordMinLength, [MinLength]);

        if (password.Length > MaxLength)
            return (false, ErrorCodes.ValidationPasswordMaxLength, [MaxLength]);

        if (!password.Any(char.IsUpper))
            return (false, ErrorCodes.ValidationPasswordUpper, []);

        if (!password.Any(char.IsLower))
            return (false, ErrorCodes.ValidationPasswordLower, []);

        if (!password.Any(char.IsDigit))
            return (false, ErrorCodes.ValidationPasswordDigit, []);

        if (!SpecialCharRegex().IsMatch(password))
            return (false, ErrorCodes.ValidationPasswordSpecial, []);

        return (true, string.Empty, []);
    }

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

    [GeneratedRegex(@"[^A-Za-z0-9]")]
    private static partial Regex SpecialCharRegex();
}
