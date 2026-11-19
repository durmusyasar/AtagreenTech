import { z } from 'zod'
import { checkPasswordStrength } from '@/core/passwordPolicy'

/** Reset-password Zod şeması — mesajlar i18n anahtarıdır. */
export const resetPasswordSchema = z
  .object({
    newPassword: z.string().min(1, 'validation.passwordRequired'),
    confirmPassword: z.string().min(1, 'invite.confirmPasswordRequired'),
  })
  .superRefine((data, ctx) => {
    if (data.newPassword && !checkPasswordStrength(data.newPassword).isValid) {
      ctx.addIssue({
        code: 'custom',
        path: ['newPassword'],
        message: 'password.policyFailed',
      })
    }

    if (data.confirmPassword && data.newPassword !== data.confirmPassword) {
      ctx.addIssue({
        code: 'custom',
        path: ['confirmPassword'],
        message: 'password.mismatch',
      })
    }
  })
