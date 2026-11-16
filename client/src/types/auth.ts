/** `POST /api/v1/auth/login` istek gövdesi. MFA açıksa `totpCode` veya `recoveryCode` gerekir. */
export interface LoginRequest {
  email: string
  password: string
  totpCode?: string
  recoveryCode?: string
}

/** `POST /api/v1/auth/login` ve `/auth/refresh` yanıtı. */
export interface AuthTokens {
  accessToken: string
  refreshToken: string
  expiresAt: string
}

/** Login sonrası karşılama / yönlendirme için özet. */
export interface LoginResult {
  accessToken: string
  refreshToken: string
  expiresAt: string
  userName: string
  tenant: string | null
  roles: string[]
}

/** UserService `AUTH_*` / `SSO_*` hata kodları. */
export const AuthErrorCodes = {
  MfaRequired: 'AUTH_MFA_REQUIRED',
  InvalidCredentials: 'AUTH_INVALID_CREDENTIALS',
  AccountLocked: 'AUTH_ACCOUNT_LOCKED',
  AccountInactive: 'AUTH_ACCOUNT_INACTIVE',
  EmailNotVerified: 'AUTH_EMAIL_NOT_VERIFIED',
} as const
