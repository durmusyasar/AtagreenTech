import { FormProvider } from 'react-hook-form'
import { AuthPageShell } from '@/Components/AuthPageShell'
import { BackToLoginLink } from '@/Components/BackToLoginLink'
import { FormFloatingInput } from '@/Components/forms/FormFloatingInput'
import { useForgotPassword } from '@modules/auth/pages/forgot-password/hooks/useForgotPassword'
import type { ForgotPasswordFormValues } from '@modules/auth/pages/forgot-password/type'

export function ForgotPasswordPage() {
  const { t, form, submittedEmail, serverError, isSubmitting, onSubmit } = useForgotPassword()

  if (submittedEmail) {
    return (
      <AuthPageShell title={t('forgot.title')}>
        <div className="space-y-4">
          <div className="auth-page-banner space-y-1 border-brand/30 bg-brand/10">
            <p className="font-semibold text-brand">{t('forgot.submit')}</p>
            <p className="text-secondary">{t('forgot.sent', submittedEmail)}</p>
          </div>
          <BackToLoginLink variant="button" />
        </div>
      </AuthPageShell>
    )
  }

  return (
    <AuthPageShell
      title={t('forgot.title')}
      description={t('forgot.description')}
      banner={serverError ? { tone: 'error', message: serverError } : undefined}
      footer={<BackToLoginLink />}
    >
      <FormProvider {...form}>
        <form onSubmit={onSubmit} noValidate className="space-y-4">
          <FormFloatingInput<ForgotPasswordFormValues>
            name="email"
            type="email"
            autoComplete="email"
            label={t('login.emailLabel')}
            autoClear
          />

          <button
            type="submit"
            disabled={isSubmitting}
            className="auth-submit mt-2 h-12 rounded-xl bg-brand px-4 text-sm font-semibold text-white shadow-[0_10px_24px_-12px_rgb(var(--brand-primary)/0.85)] transition-[background-color,box-shadow] duration-150 hover:bg-brand-hover disabled:opacity-50 disabled:shadow-none"
          >
            {isSubmitting ? t('auth.sending') : t('forgot.submit')}
          </button>
        </form>
      </FormProvider>
    </AuthPageShell>
  )
}
