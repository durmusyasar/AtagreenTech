import { App } from 'antd'
import { useCallback, useState } from 'react'
import { useLocation, useNavigate, useSearchParams } from 'react-router-dom'
import { ApiError } from '@/core/api/apiError'
import { useAuth } from '@/core/context/useAuth'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { resolveAuthErrorMessage } from '@/core/i18n/authErrors'
import { authService } from '@/services/authService'
import { AuthErrorCodes, type LoginRequest, type LoginResult } from '@/types/auth'
import type { LoginFieldErrors, LoginForm, LoginStep, MfaMode } from '@/routes/auth/login/type'
import { createLoginSchema } from '@/routes/auth/login/validation/loginSchema'

function zodIssuesToFieldErrors(
  issues: { path: PropertyKey[]; message: string }[],
): LoginFieldErrors {
  const next: LoginFieldErrors = {}
  for (const issue of issues) {
    const key = issue.path[0]
    if (key === 'email' || key === 'password' || key === 'mfaInput') {
      if (!next[key]) next[key] = issue.message
    }
  }
  return next
}

/** Login ekranı state + MFA geçiş + Zod validasyon + submit mantığı. */
export function useLoginForm() {
  const { isAuthenticated } = useAuth()
  const { language, t } = useLanguage()
  const { message } = App.useApp()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const from = (useLocation().state as { from?: string } | null)?.from ?? '/'

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [rememberMe, setRememberMe] = useState(false)
  const [mfaInput, setMfaInput] = useState('')
  const [step, setStep] = useState<LoginStep>('CREDENTIALS')
  const [mfaMode, setMfaMode] = useState<MfaMode>('TOTP')
  const [formError, setFormError] = useState<string>()
  /** Zod mesajları i18n anahtarıdır; UI `t(key)` ile çevirir (dil değişince anında). */
  const [fieldErrors, setFieldErrors] = useState<LoginFieldErrors>({})

  const validateFields = useCallback((): LoginForm | null => {
    const result = createLoginSchema(step, mfaMode).safeParse({
      email,
      password,
      mfaInput,
    })

    if (result.success) {
      setFieldErrors({})
      return { ...result.data, rememberMe }
    }

    setFieldErrors(zodIssuesToFieldErrors(result.error.issues))
    return null
  }, [email, password, mfaInput, rememberMe, step, mfaMode])

  const login = useAppMutation<LoginResult, LoginRequest>({
    mutationFn: (request) => authService.login(request),
    onSuccess: (result) => {
      message.success(t('login.welcome', result.userName))
      navigate(from, { replace: true })
    },
    onError: (error) => {
      if (error instanceof ApiError && error.code === AuthErrorCodes.MfaRequired) {
        setStep('MFA')
        setMfaMode('TOTP')
        setMfaInput('')
        setFormError(undefined)
        setFieldErrors({})
        return
      }

      if (step === 'MFA' && error instanceof ApiError && error.status === 401) {
        setFormError(
          resolveAuthErrorMessage(
            error.code === AuthErrorCodes.MfaInvalid ? error : AuthErrorCodes.MfaInvalid,
            language,
          ),
        )
        return
      }

      setFormError(resolveAuthErrorMessage(error, language))
    },
  })

  const resetToCredentials = () => {
    setStep('CREDENTIALS')
    setMfaMode('TOTP')
    setMfaInput('')
    setFormError(undefined)
    setFieldErrors({})
  }

  const switchMfaMode = () => {
    setMfaMode((current) => (current === 'TOTP' ? 'RECOVERY' : 'TOTP'))
    setMfaInput('')
    setFormError(undefined)
    setFieldErrors((prev) => ({ ...prev, mfaInput: undefined }))
  }

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault()
    setFormError(undefined)
    const values = validateFields()
    if (!values) return

    const request: LoginRequest = {
      email: values.email.trim(),
      password: values.password,
      rememberMe: values.rememberMe === true,
    }
    if (step === 'MFA') {
      const raw = values.mfaInput?.trim() ?? ''
      if (mfaMode === 'TOTP') request.totpCode = raw
      else request.recoveryCode = raw.toUpperCase()
    }
    login.mutate(request)
  }

  return {
    isAuthenticated,
    from,
    invitedBanner: searchParams.get('invited') === '1',
    resetBanner: searchParams.get('reset') === '1',
    passwordChangedBanner: searchParams.get('passwordChanged') === '1',
    email,
    setEmail,
    password,
    setPassword,
    rememberMe,
    setRememberMe,
    mfaInput,
    setMfaInput,
    step,
    mfaMode,
    formError,
    fieldErrors,
    isSubmitting: login.isPending,
    resetToCredentials,
    switchMfaMode,
    handleSubmit,
    t,
  }
}
