import { z } from 'zod'
import { refinePasswordPair } from '@/core/validation/refinePasswordPair'

/** Reset-password Zod şeması — mesajlar i18n anahtarıdır. */
export const schema = z
  .object({
    newPassword: z.string().min(1, 'validation.passwordRequired'),
    confirmPassword: z.string().min(1, 'validation.confirmPasswordRequired'),
  })
  .superRefine((data, ctx) => {
    refinePasswordPair(data.newPassword, data.confirmPassword, ctx, 'newPassword')
  })

export type ResetPasswordFormValues = z.infer<typeof schema>
