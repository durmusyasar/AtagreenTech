namespace AtaGreenTech.UserService.Application.DTOs;

/// <summary>Rol özeti ve atanmış izin adları (RBAC yönetim UI).</summary>
public sealed record RoleDetailDto(
    Guid Id,
    string Name,
    string Description,
    IReadOnlyList<string> Permissions
);
