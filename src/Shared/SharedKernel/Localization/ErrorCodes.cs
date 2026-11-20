namespace AtaGreenTech.SharedKernel.Localization;

/// <summary>
/// Yerelleştirme anahtarları — .resx key ve FE sözleşmesi ile birebir (#71).
/// Auth kodları mevcut <c>AUTH_*</c> değerlerini korur.
/// </summary>
public static class ErrorCodes
{
    public const string AuthInvalidCredentials = "AUTH_INVALID_CREDENTIALS";
    public const string AuthAccountLocked = "AUTH_ACCOUNT_LOCKED";
    public const string AuthAccountInactive = "AUTH_ACCOUNT_INACTIVE";
    public const string AuthEmailNotVerified = "AUTH_EMAIL_NOT_VERIFIED";
    public const string AuthEmailNotFound = "AUTH_EMAIL_NOT_FOUND";
    public const string AuthRateLimitExceeded = "AUTH_RATE_LIMIT_EXCEEDED";
    public const string AuthMfaRequired = "AUTH_MFA_REQUIRED";
    public const string AuthMfaInvalid = "AUTH_MFA_INVALID";
    public const string AuthBadRequest = "AUTH_BAD_REQUEST";
    public const string AuthResetTokenInvalid = "AUTH_RESET_TOKEN_INVALID";
    public const string AuthInviteTokenInvalid = "AUTH_INVITE_TOKEN_INVALID";
    public const string AuthVerifyTokenInvalid = "AUTH_VERIFY_TOKEN_INVALID";
    public const string AuthPasswordMismatch = "AUTH_PASSWORD_MISMATCH";
    public const string AuthPasswordSameAsCurrent = "AUTH_PASSWORD_SAME_AS_CURRENT";
    public const string AuthMfaAlreadyEnabled = "AUTH_MFA_ALREADY_ENABLED";
    public const string AuthMfaAlreadyDisabled = "AUTH_MFA_ALREADY_DISABLED";
    public const string AuthMfaSetupRequired = "AUTH_MFA_SETUP_REQUIRED";
    public const string SsoNotImplemented = "SSO_NOT_IMPLEMENTED";

    public const string TokenRevoked = "TOKEN_REVOKED";
    public const string SessionExpired = "SESSION_EXPIRED";
    public const string SessionReuseDetected = "SESSION_REUSE_DETECTED";
    public const string Forbidden = "FORBIDDEN";
    public const string PermissionDenied = "PERMISSION_DENIED";
    public const string ModuleNotSubscribed = "MODULE_NOT_SUBSCRIBED";

    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string ValidationRequired = "VALIDATION_REQUIRED";
    public const string ValidationMaxLength = "VALIDATION_MAX_LENGTH";
    public const string ValidationGreaterThanZero = "VALIDATION_GREATER_THAN_ZERO";
    public const string ValidationGreaterOrEqualZero = "VALIDATION_GREATER_OR_EQUAL_ZERO";
    public const string ValidationInvalid = "VALIDATION_INVALID";
    public const string ValidationMustDiffer = "VALIDATION_MUST_DIFFER";
    public const string ValidationPrecision = "VALIDATION_PRECISION";
    public const string ValidationDistinctLanguages = "VALIDATION_DISTINCT_LANGUAGES";
    public const string ValidationLanguageCode = "VALIDATION_LANGUAGE_CODE";
    public const string ValidationCurrency = "VALIDATION_CURRENCY";
    public const string ValidationSkuPattern = "VALIDATION_SKU_PATTERN";
    public const string ValidationCodePattern = "VALIDATION_CODE_PATTERN";
    public const string ValidationGtin = "VALIDATION_GTIN";
    public const string ValidationDateRange = "VALIDATION_DATE_RANGE";

    public const string ValidationPasswordMinLength = "VALIDATION_PASSWORD_MIN_LENGTH";
    public const string ValidationPasswordMaxLength = "VALIDATION_PASSWORD_MAX_LENGTH";
    public const string ValidationPasswordUpper = "VALIDATION_PASSWORD_UPPER";
    public const string ValidationPasswordLower = "VALIDATION_PASSWORD_LOWER";
    public const string ValidationPasswordDigit = "VALIDATION_PASSWORD_DIGIT";
    public const string ValidationPasswordSpecial = "VALIDATION_PASSWORD_SPECIAL";

    public const string RecordNotFound = "RECORD_NOT_FOUND";
    public const string UnexpectedError = "UNEXPECTED_ERROR";

    public const string UserCannotModifySelf = "USER_CANNOT_MODIFY_SELF";
    public const string UserLastAdmin = "USER_LAST_ADMIN";
    public const string UserEmailExists = "USER_EMAIL_EXISTS";
    public const string UserUsernameExists = "USER_USERNAME_EXISTS";
    public const string UserRoleNotFound = "USER_ROLE_NOT_FOUND";
    public const string UserRoleRequired = "USER_ROLE_REQUIRED";
    public const string UserRolesInvalid = "USER_ROLES_INVALID";

    public static class Fields
    {
        public const string Sku = "FIELD_SKU";
        public const string Barcode = "FIELD_BARCODE";
        public const string UnitCost = "FIELD_UNIT_COST";
        public const string Currency = "FIELD_CURRENCY";
        public const string Language = "FIELD_LANGUAGE";
        public const string Name = "FIELD_NAME";
        public const string Description = "FIELD_DESCRIPTION";
        public const string Product = "FIELD_PRODUCT";
        public const string Category = "FIELD_CATEGORY";
        public const string Unit = "FIELD_UNIT";
        public const string Warehouse = "FIELD_WAREHOUSE";
        public const string SourceWarehouse = "FIELD_SOURCE_WAREHOUSE";
        public const string TargetWarehouse = "FIELD_TARGET_WAREHOUSE";
        public const string Code = "FIELD_CODE";
        public const string Symbol = "FIELD_SYMBOL";
        public const string Address = "FIELD_ADDRESS";
        public const string Quantity = "FIELD_QUANTITY";
        public const string UnitPrice = "FIELD_UNIT_PRICE";
        public const string Document = "FIELD_DOCUMENT";
        public const string MovementType = "FIELD_MOVEMENT_TYPE";
        public const string Translations = "FIELD_TRANSLATIONS";
        public const string ParentCategory = "FIELD_PARENT_CATEGORY";
        public const string ProductId = "FIELD_PRODUCT_ID";
        public const string CategoryId = "FIELD_CATEGORY_ID";
        public const string UnitId = "FIELD_UNIT_ID";
        public const string WarehouseId = "FIELD_WAREHOUSE_ID";
        public const string Roles = "FIELD_ROLES";
    }
}
