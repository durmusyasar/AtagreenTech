using System.Security.Claims;
using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtaGreenTech.UserService.Infrastructure.Security;

/// <summary>
/// Claim/denylist/stamp + tenant User aktif/kilit kontrolleri (#70).
/// </summary>
internal sealed class UserAccessTokenSessionValidator(
    IAccessTokenRevocationStore revocations,
    IUserAccessStampStore stamps,
    IUserDbContext db
) : ClaimAccessTokenSessionValidator(revocations, stamps)
{
    public override async Task<AccessTokenSessionValidationResult> ValidateAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default
    )
    {
        var shared = await base.ValidateAsync(principal, cancellationToken);
        if (!shared.Succeeded)
            return shared;

        var userIdRaw = principal.FindFirstValue(JwtContract.Claims.Subject)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdRaw, out var userId))
        {
            return AccessTokenSessionValidationResult.Fail(
                AccessTokenErrorCodes.TokenRevoked,
                "Access token subject is invalid."
            );
        }

        var user = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.IsActive, u.LockoutEnd })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null || !user.IsActive)
        {
            return AccessTokenSessionValidationResult.Fail(
                AccessTokenErrorCodes.TokenRevoked,
                "User account is inactive."
            );
        }

        if (user.LockoutEnd is { } lockoutEnd && lockoutEnd > DateTime.UtcNow)
        {
            return AccessTokenSessionValidationResult.Fail(
                AccessTokenErrorCodes.TokenRevoked,
                "User account is locked."
            );
        }

        return AccessTokenSessionValidationResult.Ok();
    }
}
