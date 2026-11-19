import { FormProvider } from 'react-hook-form'
import { AuthPageShell } from '@/Components/AuthPageShell'
import { BackToLoginLink } from '@/Components/BackToLoginLink'
import { FormFloatingInput } from '@/Components/forms/FormFloatingInput'
import { useVerifyEmail } from '@/routes/auth/verify-email/hooks/useVerifyEmail'
import type { VerifyEmailFormValues } from '@/routes/auth/verify-email/type'

export function VerifyEmailPage() {
  const { t, form, status, banner, isResending, onSubmit } = useVerifyEmail()

  if (status === 'VERIFYING') {
    return (
      <AuthPageShell title={t('verify.title')}>
        <div className="py-6 text-center">
          <div
            className="mx-auto h-8 w-8 animate-spin rounded-full border-2 border-brand border-t-transparent"
            role="status"
            aria-label={t('verify.verifying')}
          />
          <p className="auth-page-description mt-4">{t('verify.verifying')}</p>
        </div>
      </AuthPageShell>
    )
  }

  if (status === 'SUCCESS') {
    return (
      <AuthPageShell title={t('verify.title')}>
        <div className="space-y-4">
          <div className="auth-page-banner space-y-1 border-brand/30 bg-brand/10">
            <p className="font-semibold text-brand">{t('verify.success')}</p>
          </div>
          <BackToLoginLink variant="button" label={t('login.submit')} />
        </div>
      </AuthPageShell>
    )
  }

  return (
    <AuthPageShell
      title={t('verify.title')}
      description={t('verify.description')}
      banner={banner}
      footer={<BackToLoginLink />}
    >
      <FormProvider {...form}>
        <form onSubmit={onSubmit} noValidate className="space-y-4">
          <FormFloatingInput<VerifyEmailFormValues>
            name="email"
            type="email"
            autoComplete="email"
            label={t('login.emailLabel')}
            autoClear
          />

          <button
            type="submit"
            disabled={isResending}
            className="auth-submit mt-2 h-12 rounded-xl bg-brand px-4 text-sm font-semibold text-white shadow-[0_10px_24px_-12px_rgb(var(--brand-primary)/0.85)] transition-[background-color,box-shadow] duration-150 hover:bg-brand-hover disabled:opacity-50 disabled:shadow-none"
          >
            {isResending ? t('auth.sending') : t('verify.resendSubmit')}
          </button>
        </form>
      </FormProvider>
    </AuthPageShell>
  )
}
