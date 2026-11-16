namespace AtaGreenTech.UserService.Application.DTOs;

/// <summary>İzin kataloğu satırı.</summary>
public sealed record PermissionDto(Guid Id, string Name, string Description);
