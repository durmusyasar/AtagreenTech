namespace AtaGreenTech.UserService.Domain.Entities;

/// <summary>
/// Tenant ↔ Module aboneliği (master DB). Issue #66 — <c>ModuleId</c> FK.
/// </summary>
public sealed class TenantModuleSubscription
{
    public Guid Id { get; init; }

    public Guid TenantId { get; init; }

    public Guid ModuleId { get; init; }

    public bool IsEnabled { get; set; }

    /// <summary>Null ise süresiz lisans.</summary>
    public DateTime? ExpiresAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}
