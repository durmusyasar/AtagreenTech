import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { ApiError } from '@/core/api/apiError'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { authPublicService } from '@/services/authPublicService'
import type { ResetPasswordFormValues } from '@/routes/auth/reset-password/type'
import { resetPasswordSchema } from '@/routes/auth/reset-password/validation/resetPasswordSchema'

const DEFAULTS: ResetPasswordFormValues = {
  newPassword: '',
  confirmPassword: '',
}

/** Yeni şifre belirleme — FormProvider + Zod + parola politikası. */
export function useResetPassword() {
  const { t } = useLanguage()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')?.trim() ?? ''
  const [serverError, setServerError] = useState<string>()

  const form = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: DEFAULTS,
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })

  const reset = useAppMutation({
    mutationFn: authPublicService.resetPassword,
    onSuccess: () => navigate('/login?reset=1', { replace: true }),
    onError: (error) => {
      setServerError(
        error instanceof ApiError && error.message ? error.message : t('reset.failed'),
      )
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
