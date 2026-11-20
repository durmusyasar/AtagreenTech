/** `POST /api/v1/auth/login` istek gövdesi. MFA açıksa `totpCode` veya `recoveryCode` gerekir. */
export interface LoginRequest {
  email: string
  password: string
  totpCode?: string
  recoveryCode?: string
  /** true → istemci token’ları localStorage’da tutar; false → sessionStorage. */
  rememberMe?: boolean
}

/** `POST /api/v1/auth/login` ve `/auth/refresh` yanıtı. */
export interface AuthTokens {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

/** Login sonrası karşılama / yönlendirme için özet. */
export type LoginResult = AuthTokens & {
  userName: string
  tenant: string | null
  roles: string[]
}

/** UserService `AUTH_*` / `SSO_*` hata kodları. */
export const AuthErrorCodes = {
  MfaRequired: 'AUTH_MFA_REQUIRED',
  MfaInvalid: 'AUTH_MFA_INVALID',
  InvalidCredentials: 'AUTH_INVALID_CREDENTIALS',
  AccountLocked: 'AUTH_ACCOUNT_LOCKED',
  AccountInactive: 'AUTH_ACCOUNT_INACTIVE',
  EmailNotVerified: 'AUTH_EMAIL_NOT_VERIFIED',
  RateLimitExceeded: 'AUTH_RATE_LIMIT_EXCEEDED',
  TenantUnresolved: 'AUTH_TENANT_UNRESOLVED',
} as const
