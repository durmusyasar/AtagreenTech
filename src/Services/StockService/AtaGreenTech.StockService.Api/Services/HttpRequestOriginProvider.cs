using AtaGreenTech.StockService.Application.Interfaces;
using AtaGreenTech.StockService.Domain.Entities;

namespace AtaGreenTech.StockService.Api.Services;

/// <summary>
/// İstek kaynağını o anki HTTP isteğinden okur. İstemci IP'si OmniSync'in forwarded headers middleware'i uygulandıktan
/// sonraki adrestir (yalnızca güvenilen proxy'lerin <c>X-Forwarded-For</c> başlığı dikkate alınır). İzleme kimliği,
/// OmniSync'in hata yanıtlarında <c>requestId</c> olarak döndürdüğü değerle aynıdır.
/// </summary>
internal sealed class HttpRequestOriginProvider(IHttpContextAccessor httpContextAccessor) : IRequestOriginProvider
{
    public MovementOrigin GetOrigin()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return MovementOrigin.None;

        var remoteIp = httpContext.Connection.RemoteIpAddress;
        if (remoteIp is { IsIPv4MappedToIPv6: true })
            remoteIp = remoteIp.MapToIPv4();

        return new MovementOrigin(
            Truncate(remoteIp?.ToString(), StockMovement.ClientIpAddressMaxLength),
            Truncate(httpContext.TraceIdentifier, StockMovement.CorrelationIdMaxLength)
        );
    }

    private static string? Truncate(string? value, int maxLength) =>
        value is { Length: var length } && length > maxLength ? value[..maxLength] : value;
}
