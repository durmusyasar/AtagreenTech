namespace AtaGreenTech.StockService.Api.Hosting;

/// <summary>
/// Container <c>HEALTHCHECK</c>'i için çalışan uygulamanın readiness endpoint'ini yoklar. Chiseled imajlarda shell,
/// curl veya wget bulunmadığından kontrol aynı imajdaki uygulama tarafından yapılır; host oluşturulmadığı için web
/// sunucusu, veritabanı bağlantısı veya konfigürasyon gerekmez.
/// </summary>
internal static class ContainerHealthProbe
{
    public const string Command = "healthcheck";

    private const string ReadyPath = "/health/ready";
    private const int DefaultHttpPort = 8080;

    // OmniSync her health check'i 5 saniyede keser; probe bundan uzun beklemelidir.
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(8);

    /// <returns>Uygulama hazırsa (Healthy veya Degraded) 0, değilse 1.</returns>
    public static async Task<int> RunAsync()
    {
        using var client = new HttpClient { Timeout = Timeout };

        try
        {
            using var response = await client.GetAsync($"http://127.0.0.1:{ResolveHttpPort()}{ReadyPath}");
            return response.IsSuccessStatusCode ? 0 : 1;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            await Console.Error.WriteLineAsync($"Health check failed: {exception.Message}");
            return 1;
        }
    }

    /// <summary>
    /// .NET container imajlarının dinleme portunu belirleyen <c>ASPNETCORE_HTTP_PORTS</c> değişkeninin ilk portu.
    /// </summary>
    private static int ResolveHttpPort()
    {
        var ports = Environment.GetEnvironmentVariable("ASPNETCORE_HTTP_PORTS");
        var first = ports?.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        return int.TryParse(first, out var port) ? port : DefaultHttpPort;
    }
}
