using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.SetupMfa
{
    /// <summary>
    /// MFA enrollment başlatır: secret + kurtarma kodları yazar, QR URI döner.
    /// Hatalarda yalnızca <see cref="ErrorCodes"/> taşır; yerelleştirme API’de (#71).
    /// </summary>
    internal sealed class SetupMfaCommandHandler(
        IRepository<User> users,
        ITotpService totpService,
        ICurrentUser currentUser,
        IOptions<AuthOptions> authOptions
    ) : ICommandHandler<SetupMfaCommand, MfaSetupResult>
    {
        public async Task<MfaSetupResult> Handle(
            SetupMfaCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!currentUser.IsAuthenticated
                || string.IsNullOrWhiteSpace(currentUser.UserId)
                || !Guid.TryParse(currentUser.UserId, out var userId))
                return MfaSetupResult.Fail(ErrorCodes.AuthInvalidCredentials);

            var user = await users.GetByIdAsync(userId, cancellationToken);
            if (user is null || !user.IsActive)
                return MfaSetupResult.Fail(ErrorCodes.AuthInvalidCredentials);

            if (user.IsMfaEnabled)
                return MfaSetupResult.Fail(ErrorCodes.AuthMfaAlreadyEnabled);

            var options = authOptions.Value;
            var recoveryCount = Math.Clamp(options.MfaRecoveryCodeCount, 1, 20);
            var secret = totpService.GenerateSecret();
            var (plaintextCodes, hashes) = RecoveryCodeFactory.Create(recoveryCount);

            user.BeginMfaEnrollment(secret, hashes);

            // Auth:MfaIssuer ValidateOnStart ile zorunlu; boşsa AuthOptions varsayılanı.
            var issuer = string.IsNullOrWhiteSpace(options.MfaIssuer)
                ? "OmniSync"
                : options.MfaIssuer.Trim();
            var uri = totpService.BuildAuthenticatorUri(user.Email, secret, issuer);

            return MfaSetupResult.Ok(new MfaSetupResponse(secret, uri, plaintextCodes));
        }
    }
}