using AtaGreenTech.UserService.Domain.Entities;
using OmniSync.Core.Domain.Repositories;

namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>Kullanıcının tüm aktif refresh oturumlarını iptal eder (şifre değişimi / sıfırlama).</summary>
internal static class SessionRevocation
{
    public static async Task RevokeAllRefreshTokensAsync(
        IRepository<RefreshToken> refreshTokens,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var active = await refreshTokens.GetAllByFilterAsync(
            rt => rt.UserId == userId && !rt.IsRevoked,
            cancellationToken
        );

        foreach (var token in active)
            token.Revoke();
    }
}
