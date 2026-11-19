import { z } from 'zod'
import { refinePasswordPair } from '@/core/validation/refinePasswordPair'

/** Backend AcceptInviteCommandValidator ile aynı. */
const NAME_MAX_LENGTH = 100

/** Accept-invite Zod şeması — mesajlar i18n anahtarıdır. */
export const schema = z
  .object({
    firstName: z
      .string()
      .trim()
      .min(1, 'invite.firstNameRequired')
      .max(NAME_MAX_LENGTH, 'validation.nameMaxLength'),
    lastName: z
      .string()
      .trim()
      .min(1, 'invite.lastNameRequired')
      .max(NAME_MAX_LENGTH, 'validation.nameMaxLength'),
    password: z.string().min(1, 'validation.passwordRequired'),
    confirmPassword: z.string().min(1, 'validation.confirmPasswordRequired'),
  })
  .superRefine((data, ctx) => {
    refinePasswordPair(data.password, data.confirmPassword, ctx, 'password')
  })

export type AcceptInviteFormValues = z.infer<typeof schema>
