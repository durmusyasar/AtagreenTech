import { FormProvider } from 'react-hook-form'
import { Link } from 'react-router-dom'
import { AuthPageShell } from '@/Components/AuthPageShell'
import { FormFloatingInput } from '@/Components/forms/FormFloatingInput'
import { useForgotPassword } from '@/routes/auth/forgot-password/hooks/useForgotPassword'
import type { ForgotPasswordFormValues } from '@/routes/auth/forgot-password/type'

export function ForgotPasswordPage() {
  const { t, form, submittedEmail, formError, isSubmitting, onSubmit } = useForgotPassword()

  return (
    <AuthPageShell title={t('forgot.title')} description={t('forgot.description')}>
      {submittedEmail ? (
        <div className="space-y-4">
          <div className="space-y-1 rounded-xl border border-brand/30 bg-brand/10 p-4 text-xs leading-relaxed">
            <p className="font-semibold text-brand">{t('forgot.submit')}</p>
            <p className="text-secondary">{t('forgot.sent', submittedEmail)}</p>
          </div>
          <Link
            to="/login"
            className="block w-full rounded-xl bg-brand px-4 py-2.5 text-center text-sm font-medium text-white transition-colors hover:bg-brand-hover"
          >
            {t('auth.backToLogin')}
          </Link>
        </div>
      ) : (
        <FormProvider {...form}>
          <form onSubmit={onSubmit} noValidate className="space-y-4">
            <FormFloatingInput<ForgotPasswordFormValues>
              name="email"
              type="email"
              autoComplete="email"
              label={t('login.emailLabel')}
              autoClear
            />

            {formError ? (
              <div
                role="alert"
                className="rounded-2xl border border-red-500/30 bg-red-500/10 p-3.5 text-xs text-red-600 dark:text-red-400"
              >
                {formError}
              </div>
            ) : null}

            <button
              type="submit"
              disabled={isSubmitting}
              className="auth-submit mt-2 h-12 rounded-xl bg-brand px-4 text-sm font-semibold text-white shadow-[0_10px_24px_-12px_rgb(var(--brand-primary)/0.85)] transition-[background-color,box-shadow] duration-150 hover:bg-brand-hover disabled:opacity-50 disabled:shadow-none"
            >
              {isSubmitting ? t('auth.sending') : t('forgot.submit')}
            </button>

            <div className="pt-1 text-center">
              <Link
                to="/login"
                className="text-xs font-medium text-secondary transition-colors hover:text-primary"
              >
                ← {t('auth.backToLogin')}
              </Link>
            </div>
          </form>
        </FormProvider>
      )}
    </AuthPageShell>
  )
}
