import { useState } from 'react'
import { FormProvider, useWatch } from 'react-hook-form'
import { AuthPageShell } from '@/Components/AuthPageShell'
import { BackToLoginLink } from '@/Components/BackToLoginLink'
import { FormFloatingInput } from '@/Components/forms/FormFloatingInput'
import { PasswordVisibilityToggle } from '@/Components/forms/PasswordVisibilityToggle'
import { PasswordStrengthList } from '@/Components/PasswordStrengthList'
import { useResetPassword } from '@/routes/auth/reset-password/hooks/useResetPassword'
import type { ResetPasswordFormValues } from '@/routes/auth/reset-password/type'

export function ResetPasswordPage() {
  const { t, form, token, serverError, isSubmitting, onSubmit } = useResetPassword()
  const [showPass, setShowPass] = useState(false)
  const newPassword = useWatch({ control: form.control, name: 'newPassword' }) ?? ''

  const banner = !token
    ? { tone: 'error' as const, message: t('reset.missingToken') }
    : serverError
      ? { tone: 'error' as const, message: serverError }
      : undefined

  return (
    <AuthPageShell title={t('reset.title')} banner={banner} footer={<BackToLoginLink />}>
      <FormProvider {...form}>
        <form onSubmit={onSubmit} noValidate className="space-y-4">
          <div className="space-y-2">
            <FormFloatingInput<ResetPasswordFormValues>
              name="newPassword"
              type={showPass ? 'text' : 'password'}
              autoComplete="new-password"
              label={t('reset.newPassword')}
              disabled={!token}
              endAdornment={
                <PasswordVisibilityToggle
                  visible={showPass}
                  onToggle={() => setShowPass((value) => !value)}
                  showLabel={t('login.showPassword')}
                  hideLabel={t('login.hidePassword')}
                  disabled={!token}
                />
              }
            />
            <PasswordStrengthList password={newPassword} />
          </div>

          <FormFloatingInput<ResetPasswordFormValues>
            name="confirmPassword"
            type={showPass ? 'text' : 'password'}
            autoComplete="new-password"
            label={t('reset.confirmPassword')}
            disabled={!token}
          />

          <button
            type="submit"
            disabled={isSubmitting || !token}
            className="auth-submit mt-2 h-12 rounded-xl bg-brand px-4 text-sm font-semibold text-white shadow-[0_10px_24px_-12px_rgb(var(--brand-primary)/0.85)] transition-[background-color,box-shadow] duration-150 hover:bg-brand-hover disabled:opacity-50 disabled:shadow-none"
          >
            {isSubmitting ? t('reset.submitting') : t('reset.submit')}
          </button>
        </form>
      </FormProvider>
    </AuthPageShell>
  )
}
