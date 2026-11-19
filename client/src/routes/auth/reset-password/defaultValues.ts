import type { ResetPasswordFormValues } from '@/routes/auth/reset-password/schema'

export const defaultValues: ResetPasswordFormValues = {
  newPassword: '',
  confirmPassword: '',
}
