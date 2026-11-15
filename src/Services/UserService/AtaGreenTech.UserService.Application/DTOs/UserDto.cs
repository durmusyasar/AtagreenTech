namespace AtaGreenTech.UserService.Application.DTOs;

/// <summary>Admin kullanıcı yönetimi için kullanıcı özeti.</summary>
public sealed record UserDto(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    bool IsActive,
    bool IsEmailVerified,
    DateTime CreatedAt,
    IReadOnlyList<string> Roles
);
