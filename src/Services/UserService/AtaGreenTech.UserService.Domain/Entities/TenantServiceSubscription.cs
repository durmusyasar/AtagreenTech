namespace AtaGreenTech.UserService.Domain.Entities;

/// <summary>
/// Tenant'ın hangi uygulama servislerine (menü gruplarına) abone olduğunu tutar. Master DB.
/// </summary>
public sealed class TenantServiceSubscription
{
    public const int ServiceNameMaxLength = 64;

    public Guid Id { get; init; }

    public Guid TenantId { get; init; }

    public string ServiceName { get; init; } = null!;

    public bool IsEnabled { get; init; }

    public DateTime SubscribedAtUtc { get; init; }
}
