using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Auth.Commands.SetupMfa;

internal sealed class SetupMfaCommandHandler(
    IRepository<User> users,
    ITotpService totpService,
    ICurrentUser currentUser,
    IOptions<AuthOptions> authOptions
) : ICommandHandler<SetupMfaCommand, MfaSetupResult>
{
    public async Task<MfaSetupResult> Handle(SetupMfaCommand request, CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated
            || string.IsNullOrWhiteSpace(currentUser.UserId)
            || !Guid.TryParse(currentUser.UserId, out var userId))
            throw new UnauthorizedAccessException("Geçerli bir oturum bulunamadı.");

        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new UnauthorizedAccessException("Kullanıcı bulunamadı.");

        if (user.IsMfaEnabled)
            return MfaSetupResult.Fail("MFA zaten etkin. Yeniden kurmak için önce kapatın.");

        var options = authOptions.Value;
        var recoveryCount = Math.Clamp(options.MfaRecoveryCodeCount, 1, 20);
        var secret = totpService.GenerateSecret();
        var (plaintextCodes, hashes) = RecoveryCodeFactory.Create(recoveryCount);

        user.BeginMfaEnrollment(secret, hashes);

        var issuer = string.IsNullOrWhiteSpace(options.MfaIssuer) ? "OmniSync" : options.MfaIssuer.Trim();
        var uri = totpService.BuildAuthenticatorUri(user.Email, secret, issuer);

        return MfaSetupResult.Ok(new MfaSetupResponse(secret, uri, plaintextCodes));
    }
}
