import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { resolveAuthErrorMessage } from '@/core/i18n/authErrors'
import { authPublicService } from '@/services/authPublicService'
import type { ForgotPasswordFormValues } from '@/routes/auth/forgot-password/type'
import { forgotPasswordSchema } from '@/routes/auth/forgot-password/validation/forgotPasswordSchema'

const DEFAULTS: ForgotPasswordFormValues = { email: '' }

/** Şifremi unuttum — FormProvider + Zod alan doğrulaması. */
export function useForgotPassword() {
  const { language, t } = useLanguage()
  const [submittedEmail, setSubmittedEmail] = useState<string>()
  const [formError, setFormError] = useState<string>()

  const form = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: DEFAULTS,
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })

  const forgot = useAppMutation({
    mutationFn: (value: string) => authPublicService.forgotPassword(value),
    onSuccess: (_data, value) => {
      setFormError(undefined)
      setSubmittedEmail(value)
    },
    onError: (error) => {
      setSubmittedEmail(undefined)
      setFormError(resolveAuthErrorMessage(error, language))
    },
  })

  const onValid = (values: ForgotPasswordFormValues) => {
    setFormError(undefined)
    forgot.mutate(values.email)
  }

  return {
    t,
    form,
    submittedEmail,
    formError,
    isSubmitting: forgot.isPending,
    onSubmit: form.handleSubmit(onValid),
  }
}
