using AtaGreenTech.UserService.Application.Features.Auth;
using OmniSync.Core.Application.Messaging;

namespace AtaGreenTech.UserService.Application.Features.Auth.Queries.GetMe;

/// <summary>
/// Kimliği doğrulanmış kullanıcının DB'den güncel profilini döner; kullanıcı yoksa veya pasifse <see langword="null"/>.
/// </summary>
public sealed record GetMeQuery : IQuery<UserMeResponse?>;
