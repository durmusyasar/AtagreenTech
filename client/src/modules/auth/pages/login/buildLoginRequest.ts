import type { LoginRequest } from '@/types/auth'
import {
  LOGIN_STEP,
  MFA_MODE,
} from '@modules/auth/pages/login/constants'
import type { LoginFormValues, LoginStep, MfaMode } from '@modules/auth/pages/login/type'

/** Form değerlerini `POST /auth/login` gövdesine çevirir. */
export function buildLoginRequest(
  values: LoginFormValues,
  step: LoginStep,
  mfaMode: MfaMode,
): LoginRequest {
  const request: LoginRequest = {
    email: values.email.trim(),
    password: values.password,
    rememberMe: values.rememberMe === true,
  }

  if (step !== LOGIN_STEP.Mfa) return request

  const raw = values.mfaInput.trim()
  if (mfaMode === MFA_MODE.Totp) request.totpCode = raw
  else request.recoveryCode = raw.toUpperCase()

  return request
}
