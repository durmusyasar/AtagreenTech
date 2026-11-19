import { z } from 'zod'
import { EMAIL_MAX_LENGTH } from '@/core/validation/emailLimits'

/** Verify-email resend Zod şeması — mesajlar i18n anahtarıdır. */
export const schema = z.object({
  email: z
    .string()
    .trim()
    .min(1, 'validation.emailRequired')
    .max(EMAIL_MAX_LENGTH, 'validation.emailMaxLength')
    .check(z.email({ error: 'validation.emailInvalid' })),
})

export type VerifyEmailFormValues = z.infer<typeof schema>
