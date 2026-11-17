import { App, Form } from 'antd'
import { useState } from 'react'
import { useLocation, useNavigate, useSearchParams } from 'react-router-dom'
import { ApiError } from '@/core/api/apiError'
import { useAuth } from '@/core/context/useAuth'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { resolveAuthErrorMessage } from '@/core/i18n/authErrors'
import { authService } from '@/services/authService'
import { AuthErrorCodes, type LoginRequest, type LoginResult } from '@/types/auth'
import type { LoginForm, LoginStep, MfaMode } from '@/routes/auth/login/type'

/** Login ekranı state + MFA geçiş + submit mantığı. */
export function useLoginForm() {
  const { isAuthenticated } = useAuth()
  const { language, t } = useLanguage()
  const { message } = App.useApp()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const from = (useLocation().state as { from?: string } | null)?.from ?? '/'

  const [step, setStep] = useState<LoginStep>('CREDENTIALS')
  const [mfaMode, setMfaMode] = useState<MfaMode>('TOTP')
  const [formError, setFormError] = useState<string>()
  const [form] = Form.useForm<LoginForm>()

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
        setFormError(undefined)
        form.setFieldValue('mfaInput', undefined)
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
    setFormError(undefined)
    form.setFieldValue('mfaInput', undefined)
  }

  const switchMfaMode = () => {
    setMfaMode((current) => (current === 'TOTP' ? 'RECOVERY' : 'TOTP'))
    setFormError(undefined)
    form.setFieldValue('mfaInput', undefined)
  }

  const submit = (values: LoginForm) => {
    setFormError(undefined)
    const request: LoginRequest = {
      email: values.email.trim(),
      password: values.password,
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
    step,
    mfaMode,
    formError,
    form,
    login,
    resetToCredentials,
    switchMfaMode,
    submit,
    t,
  }
}
