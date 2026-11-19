import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { resolveAuthFormError } from '@/core/i18n/resolveAuthFormError'
import {
  authPublicService,
  type AuthMessageResult,
  type ResetPasswordPayload,
} from '@/services/authPublicService'
import { defaultValues } from '@modules/auth/pages/reset-password/defaultValues'
import { schema, type ResetPasswordFormValues } from '@modules/auth/pages/reset-password/schema'

/** Yeni şifre formu — Zod + parola politikası; UI `FormProvider` ile sarar. */
export function useResetPassword() {
  const { language, t } = useLanguage()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')?.trim() ?? ''
  const [serverError, setServerError] = useState<string>()

  const form = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(schema),
    defaultValues,
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })

  const reset = useAppMutation<AuthMessageResult, ResetPasswordPayload>({
    mutationFn: authPublicService.resetPassword,
    onSuccess: () => navigate('/login?reset=1', { replace: true }),
    onError: (error) => {
      setServerError(resolveAuthFormError(error, language, t('reset.failed')))
    },
  })

  const onValid = (values: ResetPasswordFormValues) => {
    setServerError(undefined)

    if (!token) {
      setServerError(t('reset.missingToken'))
      return
    }

    reset.mutate({ token, newPassword: values.newPassword })
  }

  return {
    t,
    form,
    token,
    serverError,
    isSubmitting: reset.isPending,
    onSubmit: form.handleSubmit(onValid),
  }
}
