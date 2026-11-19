import { App } from 'antd'
import { useEffect, useMemo, useRef, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useLocation, useNavigate } from 'react-router-dom'
import { ApiError } from '@/core/api/apiError'
import { useAuth } from '@/core/context/useAuth'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { resolveAuthErrorMessage } from '@/core/i18n/authErrors'
import { authService } from '@/services/authService'
import { AuthErrorCodes, type LoginRequest, type LoginResult } from '@/types/auth'
import { buildLoginRequest } from '@modules/auth/pages/login/buildLoginRequest'
import {
  LOGIN_DEFAULT_REDIRECT,
  LOGIN_STEP,
  MFA_MODE,
} from '@modules/auth/pages/login/constants'
import { defaultValues } from '@modules/auth/pages/login/defaultValues'
import { createLoginSchema } from '@modules/auth/pages/login/schema'
import type { LoginFormValues, LoginStep, MfaMode } from '@modules/auth/pages/login/type'

function clearMfaField(
  form: ReturnType<typeof useForm<LoginFormValues>>,
  setFormError: (value: string | undefined) => void,
) {
  form.setValue('mfaInput', '')
  form.clearErrors('mfaInput')
  setFormError(undefined)
}

/** Login: RHF FormProvider + Zod; MFA adımı schema ref ile güncel kalır. */
export function useLoginForm() {
  const { isAuthenticated } = useAuth()
  const { language, t } = useLanguage()
  const { message } = App.useApp()
  const navigate = useNavigate()
  const from = (useLocation().state as { from?: string } | null)?.from ?? LOGIN_DEFAULT_REDIRECT

  const [step, setStep] = useState<LoginStep>(LOGIN_STEP.Credentials)
  const [mfaMode, setMfaMode] = useState<MfaMode>(MFA_MODE.Totp)
  const [formError, setFormError] = useState<string>()

  const stepRef = useRef(step)
  stepRef.current = step
  const mfaModeRef = useRef(mfaMode)
  mfaModeRef.current = mfaMode

  const schema = useMemo(() => createLoginSchema(step, mfaMode), [step, mfaMode])
  const schemaRef = useRef(schema)
  schemaRef.current = schema

  const form = useForm<LoginFormValues>({
    // Adım/mod değişince güncel Zod kurallarını kullan (ilk mount resolver’ı kilitlenmesin).
    resolver: (values, context, options) =>
      zodResolver(schemaRef.current)(values, context, options),
    defaultValues,
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })

  useEffect(() => {
    form.clearErrors()
  }, [step, mfaMode, form])

  const login = useAppMutation<LoginResult, LoginRequest>({
    mutationFn: (request) => authService.login(request),
    skipErrorNotification: true,
    onSuccess: (result) => {
      message.success(t('login.welcome', result.userName))
      navigate(from, { replace: true })
    },
    onError: (error) => {
      if (error instanceof ApiError && error.code === AuthErrorCodes.MfaRequired) {
        setStep(LOGIN_STEP.Mfa)
        setMfaMode(MFA_MODE.Totp)
        clearMfaField(form, setFormError)
        return
      }

      if (stepRef.current === LOGIN_STEP.Mfa && error instanceof ApiError && error.status === 401) {
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
    setStep(LOGIN_STEP.Credentials)
    setMfaMode(MFA_MODE.Totp)
    clearMfaField(form, setFormError)
  }

  const switchMfaMode = () => {
    setMfaMode((current) => (current === MFA_MODE.Totp ? MFA_MODE.Recovery : MFA_MODE.Totp))
    clearMfaField(form, setFormError)
  }

  const onValid = (values: LoginFormValues) => {
    setFormError(undefined)
    login.mutate(buildLoginRequest(values, stepRef.current, mfaModeRef.current))
  }

  return {
    isAuthenticated,
    from,
    form,
    step,
    mfaMode,
    formError,
    isSubmitting: login.isPending,
    resetToCredentials,
    switchMfaMode,
    onSubmit: form.handleSubmit(onValid),
  }
}
