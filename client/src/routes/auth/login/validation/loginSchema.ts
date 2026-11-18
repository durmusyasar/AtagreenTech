import { z } from 'zod'
import {
  LOGIN_PASSWORD_MIN_LENGTH,
  LOGIN_STEP,
  MFA_MODE,
  RECOVERY_CODE_PATTERN,
  TOTP_CODE_PATTERN,
} from '@/routes/auth/login/constants'
import type { LoginStep, MfaInputMode } from '@/routes/auth/login/type'

/**
 * Login Zod şeması — alan adları RHF `name` ile birebir.
 * Mesajlar i18n anahtarıdır; UI `t(key)` ile çevirir.
 */
export function createLoginSchema(step: LoginStep, mfaMode: MfaInputMode) {
  return z
    .object({
      email: z
        .string()
        .trim()
        .min(1, 'validation.emailRequired')
        .check(z.email({ error: 'validation.emailInvalid' })),
      password: z
        .string()
        .min(1, 'validation.passwordRequired')
        .refine((value) => value.length >= LOGIN_PASSWORD_MIN_LENGTH, {
          message: 'validation.passwordMinLength',
        }),
      rememberMe: z.boolean(),
      mfaInput: z.string(),
    })
    .superRefine((data, ctx) => {
      if (step !== LOGIN_STEP.Mfa) return

      const code = data.mfaInput.trim()
      if (mfaMode === MFA_MODE.Totp) {
        if (!TOTP_CODE_PATTERN.test(code)) {
          ctx.addIssue({
            code: 'custom',
            path: ['mfaInput'],
            message: 'validation.totpInvalid',
          })
        }
        return
      }

      if (!RECOVERY_CODE_PATTERN.test(code)) {
        ctx.addIssue({
          code: 'custom',
          path: ['mfaInput'],
          message: 'validation.recoveryInvalid',
        })
      }
    })
}

export type LoginSchemaValues = z.infer<ReturnType<typeof createLoginSchema>>
