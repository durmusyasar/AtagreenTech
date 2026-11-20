using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AtaGreenTech.SharedKernel.Security;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using Microsoft.Extensions.Options;
using OmniSync.Core.Infrastructure.Authentication;

namespace AtaGreenTech.UserService.Infrastructure.Security
{

    /// <summary>
    /// Access token'ları OmniSync <see cref="JwtTokenSigning"/> ile imzalar; claim adları <see cref="JwtContract"/> ile aynıdır
    /// (<c>sub</c>, <c>email</c>, <c>tenant_id</c>, <c>sid</c>, <c>sst</c>, <c>role</c>, <c>permission</c>).
    /// </summary>
    internal sealed class JwtAccessTokenIssuer(
        IOptions<JwtOptions> jwtOptions,
        IOptions<AuthOptions> authOptions
    ) : IAccessTokenIssuer
    {
        /// <inheritdoc />
        public AccessTokenResult Create(
            Guid userId,
            string email,
            Guid tenantId,
            Guid sessionId,
            string securityStamp,
            IReadOnlyCollection<string> roles,
            IReadOnlyCollection<string> permissions
        )
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User id is required.", nameof(userId));
            if (tenantId == Guid.Empty)
                throw new ArgumentException("Tenant id is required.", nameof(tenantId));
            if (sessionId == Guid.Empty)
                throw new ArgumentException("Session id is required.", nameof(sessionId));
            ArgumentException.ThrowIfNullOrWhiteSpace(email);
            ArgumentException.ThrowIfNullOrWhiteSpace(securityStamp);
            ArgumentNullException.ThrowIfNull(roles);
            ArgumentNullException.ThrowIfNull(permissions);

            var jwt = jwtOptions.Value;
            var minutes = authOptions.Value.AccessTokenMinutes;
            if (minutes <= 0)
                minutes = JwtContract.DefaultAccessTokenMinutes;

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(minutes);
            var claims = new List<Claim>
            {
                new(JwtContract.Claims.Subject, userId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtContract.Claims.Email, email.Trim()),
                new(JwtContract.Claims.TenantId, tenantId.ToString()),
                new(JwtContract.Claims.SessionId, sessionId.ToString()),
                new(JwtContract.Claims.SecurityStamp, securityStamp.Trim()),
            };

            foreach (var role in roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(role))
                    claims.Add(new Claim(JwtContract.Claims.Role, role.Trim()));
            }

            foreach (var permission in permissions.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(permission))
                    claims.Add(new Claim(JwtContract.Claims.Permission, permission.Trim()));
            }

            var credentials = JwtTokenSigning.CreateSigningCredentials(jwt);
            var token = new JwtSecurityToken(
                issuer: jwt.Issuer,
                audience: jwt.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: credentials
            );

            return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
        }
    }
}