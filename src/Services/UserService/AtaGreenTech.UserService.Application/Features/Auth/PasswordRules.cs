using System.Text.RegularExpressions;
using FluentValidation;

namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>
/// Güçlü şifre kuralları: min 8 karakter, büyük/küçük harf, rakam ve özel karakter.
/// </summary>
public static partial class PasswordRules
{
    public const int MinLength = 8;
    public const int MaxLength = 128;

    public static (bool IsValid, string Message) Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinLength)
            return (false, "Şifre en az 8 karakter uzunluğunda olmalıdır.");

        if (password.Length > MaxLength)
            return (false, $"Şifre en fazla {MaxLength} karakter olabilir.");

        if (!password.Any(char.IsUpper))
            return (false, "Şifre en az bir büyük harf içermelidir.");

        if (!password.Any(char.IsLower))
            return (false, "Şifre en az bir küçük harf içermelidir.");

        if (!password.Any(char.IsDigit))
            return (false, "Şifre en az bir rakam içermelidir.");

        if (!SpecialCharRegex().IsMatch(password))
            return (false, "Şifre en az bir özel karakter içermelidir.");

        return (true, string.Empty);
    }

    public static IRuleBuilderOptionsConditions<T, string> ApplyStrongPassword<T>(
        this IRuleBuilder<T, string> rule
    ) =>
        rule.Custom(
            (password, context) =>
            {
                var (isValid, message) = Validate(password);
                if (!isValid)
                    context.AddFailure(message);
            }
        );

    [GeneratedRegex(@"[^A-Za-z0-9]")]
    private static partial Regex SpecialCharRegex();
}
