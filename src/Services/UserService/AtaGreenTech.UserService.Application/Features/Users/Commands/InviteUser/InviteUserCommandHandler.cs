using AtaGreenTech.SharedKernel.Localization;
using System.Security.Cryptography;
using AtaGreenTech.UserService.Application.Features.Auth;
using AtaGreenTech.UserService.Application.Interfaces;
using AtaGreenTech.UserService.Application.Options;
using AtaGreenTech.UserService.Domain.Entities;
using Microsoft.Extensions.Options;
using OmniSync.Core.Application.Messaging;
using OmniSync.Core.Domain.Repositories;
using OmniSync.Core.Domain.Tenants;
using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Application.Features.Users.Commands.InviteUser;

internal sealed class InviteUserCommandHandler(
    IRepository<User> users,
    IRepository<Role> roles,
    IRepository<UserRole> userRoles,
    IPasswordHasher passwordHasher,
    IUserInvitationSender invitationSender,
    IIdentityAuditLogger auditLogger,
    ICurrentUser currentUser,
    ICurrentTenant currentTenant,
    IOptions<AuthOptions> authOptions,
    IErrorLocalizer localizer
) : ICommandHandler<InviteUserCommand, MessageResult>
{
    public async Task<MessageResult> Handle(InviteUserCommand request, CancellationToken cancellationToken)
    {
        var email = User.NormalizeEmail(request.Email);
        if (await users.ExistsAsync(u => u.Email == email, cancellationToken))
            return MessageResult.Fail(ErrorCodes.UserEmailExists);

        var roleName = Role.NormalizeName(request.Role);
        var knownRoles = await roles.GetAllByFilterAsync(
            r => r.Name == Role.Names.Admin || r.Name == Role.Names.Manager || r.Name == Role.Names.Viewer,
            cancellationToken
        );
        var role = knownRoles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        if (role is null)
            return MessageResult.Fail(ErrorCodes.UserRoleNotFound);

        var username = ResolveUsername(email);
        if (await users.ExistsAsync(u => u.Username == username, cancellationToken))
            return MessageResult.Fail(ErrorCodes.UserUsernameExists);

        var days = Math.Max(1, authOptions.Value.InvitationDays);
        var (plaintext, hash) = EmailVerificationTokenFactory.Create();
        // Login'i engelleyen rastgele özet; accept-invite gerçek şifreyi yazar.
        var pendingHash = passwordHasher.Hash(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

        var user = User.CreateInvited(
            email,
            username,
            pendingHash,
            request.FirstName,
            request.LastName,
            hash,
            DateTime.UtcNow.AddDays(days)
        );

        await users.AddAsync(user, cancellationToken);
        await userRoles.AddAsync(new UserRole(user.Id, role.Id), cancellationToken);

        var inviteUrl = AuthPublicLinks.Build(
            authOptions.Value,
            currentTenant,
            "/accept-invite",
            plaintext
        );
        await invitationSender.SendAsync(user.Email, inviteUrl, cancellationToken);

        var actor = string.IsNullOrWhiteSpace(currentUser.Email) ? currentUser.UserId : currentUser.Email;
        var actorLabel = string.IsNullOrWhiteSpace(actor)
            ? localizer.Localize(ErrorCodes.Audit.ActorUnknown)
            : actor;
        await auditLogger.LogAsync(
            AuditLog.Actions.UserInvited,
            user.Email,
            user.Id,
            request.ClientIp,
            localizer.Localize(ErrorCodes.Audit.UserInvited, fallback: null, role.Name, actorLabel),
            cancellationToken
        );

        return MessageResult.Ok("Kullanıcı daveti başarıyla gönderildi.");
    }

    private static string ResolveUsername(string email)
    {
        if (email.Length <= User.UsernameMaxLength)
            return email;

        var local = email.Split('@')[0];
        return local.Length <= User.UsernameMaxLength
            ? local
            : local[..User.UsernameMaxLength];
    }
}
