using AtaGreenTech.SharedKernel.Localization;
using AtaGreenTech.UserService.Application.Features.Auth;

namespace AtaGreenTech.UserService.Tests.Application;

public sealed class AuthAttemptResultTests
{
    [Fact]
    public void Ok_is_success_with_response()
    {
        var response = new AuthResponse("access", "refresh", DateTime.UtcNow.AddHours(1));
        var result = AuthAttemptResult.Ok(response);

        Assert.True(result.IsSuccess);
        Assert.Same(response, result.Response);
        Assert.Null(result.ErrorCode);
        Assert.False(result.MfaRequired);
    }

    [Fact]
    public void Fail_carries_error_code_without_user_message()
    {
        var result = AuthAttemptResult.Fail(AuthErrorCodes.InvalidCredentials);

        Assert.False(result.IsSuccess);
        Assert.Equal(AuthErrorCodes.InvalidCredentials, result.ErrorCode);
        Assert.Null(result.ErrorArgs);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public void Fail_account_locked_includes_minutes_arg()
    {
        var result = AuthAttemptResult.Fail(AuthErrorCodes.AccountLocked, 423, 15);

        Assert.Equal(AuthErrorCodes.AccountLocked, result.ErrorCode);
        Assert.Equal(423, result.StatusCode);
        Assert.Equal([15], result.ErrorArgs);
    }

    [Fact]
    public void Fail_tenant_unresolved_uses_stable_auth_code()
    {
        var result = AuthAttemptResult.Fail(AuthErrorCodes.TenantUnresolved, 400);

        Assert.Equal(ErrorCodes.AuthTenantUnresolved, result.ErrorCode);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public void RequireMfa_sets_flag_and_auth_code()
    {
        var result = AuthAttemptResult.RequireMfa();

        Assert.False(result.IsSuccess);
        Assert.True(result.MfaRequired);
        Assert.Equal(AuthErrorCodes.MfaRequired, result.ErrorCode);
        Assert.Equal(401, result.StatusCode);
    }
}
