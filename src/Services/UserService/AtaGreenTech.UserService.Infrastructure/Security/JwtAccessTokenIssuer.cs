using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OmniSync.Core.Infrastructure.Authentication;

namespace AtaGreenTech.UserService.Infrastructure.Security;

/// <summary>
/// Access token'ları OmniSync <see cref="JwtTokenSigning"/> ile imzalar; claim adları <see cref="JwtContract"/> ile aynıdır.
/// </summary>
internal sealed class JwtAccessTokenIssuer(
    IOptions<JwtOptions> jwtOptions,
    IOptions<AuthOptions> authOptions
) : IAccessTokenIssuer
{
    public AccessTokenResult Create(Guid userId, string email, Guid tenantId, IReadOnlyCollection<string> roles)
    {
        var jwt = jwtOptions.Value;
        var minutes = authOptions.Value.AccessTokenMinutes;
        if (minutes <= 0)
            minutes = JwtContract.DefaultAccessTokenMinutes;

        var expires = DateTime.UtcNow.AddMinutes(minutes);
        var claims = new List<Claim>
        {
            new(JwtContract.Claims.Subject, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtContract.Claims.Email, email),
            new(JwtContract.Claims.TenantId, tenantId.ToString()),
        };

        foreach (var role in roles)
            claims.Add(new Claim(JwtContract.Claims.Role, role));

        var credentials = JwtTokenSigning.CreateSigningCredentials(jwt);
        var token = new JwtSecurityToken(
            issuer: jwt.Issuer,
            audience: jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials
        );

        return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
