using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using OmniSync.Core.Domain.Exceptions;

namespace AtaGreenTech.SharedKernel.Localization;

/// <summary>
/// OmniSync <see cref="ApplicationValidationExceptionWithResourceKey"/> için app-owned 422 yanıtı (#71 B).
/// <c>detail</c> SharedResource ile yerelleştirilir; <c>resourceKey</c> / <c>parameters</c> korunur (FE fallback).
/// </summary>
public sealed class LocalizedBusinessRuleExceptionHandler(IErrorLocalizer localizer) : IExceptionHandler
{
    public const string BusinessRuleViolationCode = "business_rule_violation";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is not ApplicationValidationExceptionWithResourceKey businessRule)
            return false;

        var args = businessRule.Parameters ?? [];
        var detail = localizer.Localize(businessRule.ResourceKey, fallback: null, args);

        httpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "Unprocessable Entity",
                status = StatusCodes.Status422UnprocessableEntity,
                detail,
                errorCode = BusinessRuleViolationCode,
                resourceKey = businessRule.ResourceKey,
                parameters = args.Select(static p => p?.ToString() ?? string.Empty).ToArray(),
                traceId = httpContext.TraceIdentifier,
            },
            cancellationToken
        );

        return true;
    }
}
