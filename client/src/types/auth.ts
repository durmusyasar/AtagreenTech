/**
 * Kimlik servisi gelene kadar oturum, stok servisinin doğrulayacağı erişim token'ıyla kurulur.
 * Tenant adresten (veya geliştirmede `VITE_DEV_TENANT`) gelir; istek gövdesinde taşınmaz.
 */
export interface LoginRequest {
  accessToken: string
}

/** Korunan uçtan dönen oturum özeti; arayüzde karşılama ve doğrulama için kullanılır. */
export interface LoginResult {
  token: string
  userName: string
  tenant: string | null
  roles: string[]
}
