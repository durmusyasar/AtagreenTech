import { z } from 'zod'

/** Forgot-password Zod şeması — mesajlar i18n anahtarıdır. */
export const forgotPasswordSchema = z.object({
  email: z
    .string()
    .trim()
    .min(1, 'validation.emailRequired')
    .check(z.email({ error: 'validation.emailInvalid' })),
})
