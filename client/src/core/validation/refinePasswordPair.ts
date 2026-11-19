import { z } from 'zod'
import { checkPasswordStrength } from '@/core/passwordPolicy'

/**
 * Parola politikası + onay eşleşmesi — accept-invite / reset-password ortak.
 * Boş alanlarda `min(1)` hatasını çift göstermemek için guard’lar korunur.
 */
export function refinePasswordPair(
  password: string,
  confirmPassword: string,
  ctx: z.RefinementCtx,
  passwordPath: 'password' | 'newPassword',
): void {
  if (password && !checkPasswordStrength(password).isValid) {
    ctx.addIssue({
      code: 'custom',
      path: [passwordPath],
      message: 'password.policyFailed',
    })
  }

  if (confirmPassword && password !== confirmPassword) {
    ctx.addIssue({
      code: 'custom',
      path: ['confirmPassword'],
      message: 'password.mismatch',
    })
  }
}
