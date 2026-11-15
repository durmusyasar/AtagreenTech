namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>Başarılı login yanıtı: access JWT + tek kullanımlık refresh (Issue #27'de rotation).</summary>
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
