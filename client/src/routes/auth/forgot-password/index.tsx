import { Link } from 'react-router-dom'
import { AuthPageShell } from '@/Components/AuthPageShell'
import { FloatingInput } from '@/Components/FloatingInput'
import { useForgotPassword } from '@/routes/auth/forgot-password/hooks/useForgotPassword'

export function ForgotPasswordPage() {
  const { t, email, setEmail, fieldErrors, submittedEmail, isSubmitting, handleSubmit } =
    useForgotPassword()

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
        <form onSubmit={handleSubmit} noValidate className="space-y-4">
          <FloatingInput
            id="reset-email"
            type="email"
            autoComplete="email"
            label={t('login.emailLabel')}
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            error={fieldErrors.email ? t(fieldErrors.email) : undefined}
            autoClear
            clearLabel={t('common.clear')}
          />

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
      )}
    </AuthPageShell>
  )
}
