using OmniSync.Core.Domain.Users;

namespace AtaGreenTech.UserService.Tests.Fakes;

internal sealed class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; init; }
    public string? UserId { get; init; }
    public string? Email { get; init; }
    public IReadOnlyCollection<string> Roles { get; init; } = [];
}

