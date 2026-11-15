namespace AtaGreenTech.UserService.Application.DTOs;

/// <summary>Kullanıcının aktif refresh oturumu özeti.</summary>
public sealed record SessionDto(
    Guid Id,
    string? IpAddress,
    string? DeviceInfo,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsCurrent
);
