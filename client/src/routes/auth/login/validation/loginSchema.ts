import { z } from 'zod'
import type { LoginStep, MfaInputMode } from '@/routes/auth/login/type'

/**
 * Login Zod şeması — mesajlar i18n anahtarıdır; UI `t(key)` ile çevirir.
 * Böylece dil değişince re-parse gerekmez.
 */
export function createLoginSchema(step: LoginStep, mfaMode: MfaInputMode) {
  return z
    .object({
      email: z
        .string()
        .trim()
        .min(1, 'validation.emailRequired')
        // Zod 4: top-level z.email(); "asd" gibi değerler invalid_format ile elenir.
        .check(z.email({ error: 'validation.emailInvalid' })),
      password: z
        .string()
        .min(1, 'validation.passwordRequired')
        .refine((value) => value.length >= 6, {
          message: 'validation.passwordMinLength',
        }),
      mfaInput: z.string().optional(),
    })
    .superRefine((data, ctx) => {
      if (step !== 'MFA') return

      const code = (data.mfaInput ?? '').trim()
      if (mfaMode === 'TOTP') {
        if (!/^\d{6}$/.test(code)) {
          ctx.addIssue({
            code: 'custom',
            path: ['mfaInput'],
            message: 'validation.totpInvalid',
          })
        }
        return
      }

      if (!/^[A-Za-z0-9]{8}$/.test(code)) {
        ctx.addIssue({
          code: 'custom',
          path: ['mfaInput'],
          message: 'validation.recoveryInvalid',
        })
      }
    })
}

export type LoginSchemaValues = z.infer<ReturnType<typeof createLoginSchema>>
