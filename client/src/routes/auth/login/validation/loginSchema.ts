import { z } from 'zod'
import {
  LOGIN_PASSWORD_MIN_LENGTH,
  LOGIN_STEP,
  LOGIN_VALIDATION,
  MFA_MODE,
  RECOVERY_CODE_PATTERN,
  TOTP_CODE_PATTERN,
} from '@/routes/auth/login/constants'
import type { LoginFormValues, LoginStep, MfaInputMode } from '@/routes/auth/login/type'

function mfaInputSchema(mfaMode: MfaInputMode) {
  if (mfaMode === MFA_MODE.Totp) {
    return z
      .string()
      .trim()
      .regex(TOTP_CODE_PATTERN, LOGIN_VALIDATION.totpInvalid)
  }

  return z
    .string()
    .trim()
    .regex(RECOVERY_CODE_PATTERN, LOGIN_VALIDATION.recoveryInvalid)
}

/**
 * Login Zod şeması — alan adları RHF `name` ile birebir.
 * Mesajlar i18n anahtarıdır; UI `t(key)` ile çevirir.
 */
export function createLoginSchema(step: LoginStep, mfaMode: MfaInputMode) {
  return z.object({
    email: z
      .string()
      .trim()
      .min(1, LOGIN_VALIDATION.emailRequired)
      .check(z.email({ error: LOGIN_VALIDATION.emailInvalid })),
    password: z
      .string()
      .min(1, LOGIN_VALIDATION.passwordRequired)
      .min(LOGIN_PASSWORD_MIN_LENGTH, LOGIN_VALIDATION.passwordMinLength),
    rememberMe: z.boolean(),
    mfaInput:
      step === LOGIN_STEP.Mfa ? mfaInputSchema(mfaMode) : z.string(),
  })
}

/** Form değerleriyle aynı şekil; infer yerine tek kaynak. */
export type LoginSchemaValues = LoginFormValues
