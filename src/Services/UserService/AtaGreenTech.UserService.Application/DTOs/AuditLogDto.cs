namespace AtaGreenTech.UserService.Application.DTOs;

public sealed record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string Email,
    string Action,
    string? IpAddress,
    string? Details,
    DateTime CreatedAt
);
