using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Queries.GetMe;

/// <summary>
/// Kimliği doğrulanmış kullanıcının DB’den güncel profilini döner.
/// Oturum/tenant/pasif kullanıcı hatalarında <see cref="GetMeResult.ErrorCode"/> taşır (#71).
/// </summary>
public sealed record GetMeQuery : IQuery<GetMeResult>;
