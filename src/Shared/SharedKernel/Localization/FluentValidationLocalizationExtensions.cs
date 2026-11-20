using FluentValidation;

namespace AtaGreenTech.SharedKernel.Localization;

/// <summary>FluentValidation kurallarını <see cref="IErrorLocalizer"/> ile bağlar (#71 PR-B).</summary>
public static class FluentValidationLocalizationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithLocalizedMessage<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        IErrorLocalizer localizer,
        string errorCode,
        params object[] args
    ) =>
        rule.WithMessage(_ => localizer.Localize(errorCode, args));

    public static IRuleBuilderOptions<T, TProperty> WithRequiredMessage<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        IErrorLocalizer localizer,
        string fieldCode
    ) =>
        rule.WithLocalizedMessage(
            localizer,
            ErrorCodes.ValidationRequired,
            localizer.Localize(fieldCode)
        );

    public static IRuleBuilderOptions<T, TProperty> WithMaxLengthMessage<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule,
        IErrorLocalizer localizer,
        string fieldCode,
        int maxLength
    ) =>
        rule.WithLocalizedMessage(
            localizer,
            ErrorCodes.ValidationMaxLength,
            localizer.Localize(fieldCode),
            maxLength
        );
}
