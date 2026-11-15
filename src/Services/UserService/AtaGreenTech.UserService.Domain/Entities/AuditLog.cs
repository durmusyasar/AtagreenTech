using OmniSync.Core.Domain.Common;

namespace AtaGreenTech.UserService.Domain.Entities;

/// <summary>
/// Kimlik / güvenlik denetim kaydı. Append-only; soft-delete uygulanmaz (BaseEntity uyumu için alanlar vardır).
/// </summary>
public class AuditLog : BaseEntity
{
    public const int EmailMaxLength = 256;
    public const int ActionMaxLength = 64;
    public const int IpAddressMaxLength = 64;
    public const int DetailsMaxLength = 2000;

    protected AuditLog()
    {
        Email = null!;
        Action = null!;
    }

    public AuditLog(
        string action,
        string email,
        Guid? userId = null,
        string? ipAddress = null,
        string? details = null
    )
        : this()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        Action = NormalizeAction(action);
        if (Action.Length > ActionMaxLength)
            throw new ArgumentException($"Action must be at most {ActionMaxLength} characters.", nameof(action));

        Email = User.NormalizeEmail(email);
        if (Email.Length > EmailMaxLength)
            throw new ArgumentException($"Email must be at most {EmailMaxLength} characters.", nameof(email));

        UserId = userId;
        IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
        if (IpAddress is { Length: > IpAddressMaxLength })
            IpAddress = IpAddress[..IpAddressMaxLength];

        Details = string.IsNullOrWhiteSpace(details) ? null : details.Trim();
        if (Details is { Length: > DetailsMaxLength })
            Details = Details[..DetailsMaxLength];
    }

    public Guid? UserId { get; private set; }

    public string Email { get; private set; }

    /// <summary>Örn. <c>LOGIN_SUCCESS</c>, <c>ROLE_CHANGED</c>.</summary>
    public string Action { get; private set; }

    public string? IpAddress { get; private set; }

    public string? Details { get; private set; }

    public static string NormalizeAction(string action) => action.Trim().ToUpperInvariant();

    /// <summary>Standart kimlik denetim eylemleri.</summary>
    public static class Actions
    {
        public const string LoginSuccess = "LOGIN_SUCCESS";
        public const string LoginFailure = "LOGIN_FAILURE";
        public const string RoleChanged = "ROLE_CHANGED";
        public const string UserInvited = "USER_INVITED";
        public const string PasswordReset = "PASSWORD_RESET";
    }
}
