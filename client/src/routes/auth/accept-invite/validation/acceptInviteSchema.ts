import { z } from 'zod'
import { checkPasswordStrength } from '@/core/passwordPolicy'

/** Accept-invite Zod şeması — mesajlar i18n anahtarıdır. */
export const acceptInviteSchema = z
  .object({
    firstName: z.string().trim().min(1, 'invite.firstNameRequired'),
    lastName: z.string().trim().min(1, 'invite.lastNameRequired'),
    password: z.string().min(1, 'validation.passwordRequired'),
    confirmPassword: z.string().min(1, 'invite.confirmPasswordRequired'),
  })
  .superRefine((data, ctx) => {
    if (data.password && !checkPasswordStrength(data.password).isValid) {
      ctx.addIssue({
        code: 'custom',
        path: ['password'],
        message: 'password.policyFailed',
      })
    }

    if (data.confirmPassword && data.password !== data.confirmPassword) {
      ctx.addIssue({
        code: 'custom',
        path: ['confirmPassword'],
        message: 'password.mismatch',
      })
    }
  })
