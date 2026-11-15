/**
 * Kimlik servisi gelene kadar kullanılan varsayılan erişim token'ı.
 * Yalnızca derleme / geliştirme ortam değişkeninden okunur; repoya gömülmez.
 */
export function defaultAccessToken(): string {
  return import.meta.env.VITE_DEFAULT_ACCESS_TOKEN?.trim() ?? ''
}
