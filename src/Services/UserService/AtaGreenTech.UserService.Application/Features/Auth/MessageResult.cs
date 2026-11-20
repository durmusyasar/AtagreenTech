namespace AtaGreenTech.UserService.Application.Features.Auth
{
    /// <summary>
    /// Basit başarı/hata sonucu. Hata ve kodlu başarıda <see cref="ErrorCode"/> taşır;
    /// yerelleştirme API sınırında yapılır (#71).
    /// </summary>
    public sealed record MessageResult(
        bool Succeeded,
        string Message,
        string? ErrorCode = null,
        IReadOnlyList<object>? ErrorArgs = null)
    {
        /// <summary>Ham başarı metni (geçiş dönemi; yeni kodlar <see cref="OkCode"/> kullanır).</summary>
        public static MessageResult Ok(string message) => new(true, message);

        /// <summary>Başarı kodu; API <see cref="ErrorCode"/> üzerinden lokalize eder.</summary>
        public static MessageResult OkCode(string successCode) => new(true, successCode, successCode);

        /// <summary>
        /// Hata kodu taşır. Tek argüman her zaman <see cref="ErrorCode"/> olarak yazılır
        /// (eski <c>Fail(string message)</c> aşırı yüklemesi kaldırıldı — aksi halde kod kayboluyordu).
        /// </summary>
        public static MessageResult Fail(string errorCode, string? fallbackMessage = null, params object[] args) =>
            new(false, fallbackMessage ?? errorCode, errorCode, args);
    }
}