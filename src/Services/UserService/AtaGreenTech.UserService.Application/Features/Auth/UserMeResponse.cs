namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>
/// Giriş yapmış kullanıcının güncel profili; session hydration için <c>GET /api/v1/auth/me</c> yanıtı.
/// <paramref name="TenantId"/> JWT / host çözümlemesinden gelir (User entity'de kolon yok).
/// </summary>
public sealed record UserMeResponse(
    Guid Id,
    string Email,
    string Username,
    string FirstName,
    string LastName,
    string TenantId,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions
);
