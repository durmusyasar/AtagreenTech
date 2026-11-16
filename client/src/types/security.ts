/** `GET /api/v1/sessions` satırı (`SessionDto`). */
export interface ActiveSessionItem {
  id: string
  ipAddress: string | null
  deviceInfo: string | null
  createdAt: string
  expiresAt: string
  isCurrent: boolean
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

/** `POST /api/v1/auth/mfa/setup` yanıtı. */
export interface MfaSetupData {
  sharedKey: string
  qrCodeUri: string
  recoveryCodes: string[]
}
