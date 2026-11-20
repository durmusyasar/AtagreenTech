using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.Extensions.Options;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;

namespace AtaGreenTech.UserService.Application.Features.Auth;

/// <summary>Access stamp yayınlama ve oturum denylist işlemleri (#70).</summary>
public sealed class AccessTokenLifecycle(
    IUserAccessStampStore stamps,
    IAccessTokenRevocationStore revocations,
    ICurrentTenant currentTenant,
    IOptions<AuthOptions> authOptions
)
{
    public TimeSpan AccessTokenTtl
    {
        get
        {
            var minutes = authOptions.Value.AccessTokenMinutes;
            if (minutes <= 0)
                minutes = JwtContract.DefaultAccessTokenMinutes;
            return TimeSpan.FromMinutes(minutes);
        }
    }

    public async Task PublishStampAsync(
        Guid userId,
        string securityStamp,
        CancellationToken cancellationToken
    )
    {
        if (currentTenant.Id is not { } tenantId)
            throw new InvalidOperationException("Tenant could not be resolved from the host.");

        await stamps.PublishAsync(tenantId, userId, securityStamp, cancellationToken);
    }

    public Task RevokeSessionAsync(Guid sessionId, CancellationToken cancellationToken) =>
        revocations.RevokeSessionAsync(sessionId, AccessTokenTtl, cancellationToken);

    public async Task RevokeFamiliesAsync(
        IEnumerable<Guid> familyIds,
        CancellationToken cancellationToken
    )
    {
        foreach (var familyId in familyIds.Distinct())
            await RevokeSessionAsync(familyId, cancellationToken);
    }

    public async Task RevokeAllSessionsAsync(
        IRepository<RefreshToken> refreshTokens,
        Guid userId,
        CancellationToken cancellationToken
    )
    {
        var active = await refreshTokens.GetAllByFilterAsync(
            rt => rt.UserId == userId && !rt.IsRevoked,
            cancellationToken
        );

        var families = new HashSet<Guid>();
        foreach (var token in active)
        {
            families.Add(token.FamilyId);
            token.Revoke();
        }

        await RevokeFamiliesAsync(families, cancellationToken);
    }
}
