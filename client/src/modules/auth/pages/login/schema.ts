import { z } from 'zod'
import { EMAIL_MAX_LENGTH } from '@/core/validation/emailLimits'
import {
  LOGIN_PASSWORD_MIN_LENGTH,
  LOGIN_STEP,
  LOGIN_VALIDATION,
  MFA_MODE,
  RECOVERY_CODE_PATTERN,
  TOTP_CODE_PATTERN,
} from '@modules/auth/pages/login/constants'
import type { LoginFormValues, LoginStep, MfaMode } from '@modules/auth/pages/login/type'

function mfaInputSchema(mfaMode: MfaMode) {
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
export function createLoginSchema(step: LoginStep, mfaMode: MfaMode) {
  return z.object({
    email: z
      .string()
      .trim()
      .min(1, LOGIN_VALIDATION.emailRequired)
      .max(EMAIL_MAX_LENGTH, LOGIN_VALIDATION.emailMaxLength)
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
