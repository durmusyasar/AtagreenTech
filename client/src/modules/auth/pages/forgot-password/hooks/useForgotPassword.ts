import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { resolveAuthErrorMessage } from '@/core/i18n/authErrors'
import { authPublicService, type AuthMessageResult } from '@/services/authPublicService'
import { defaultValues } from '@modules/auth/pages/forgot-password/defaultValues'
import { schema, type ForgotPasswordFormValues } from '@modules/auth/pages/forgot-password/schema'

/** Şifremi unuttum formu — Zod alan doğrulaması; UI `FormProvider` ile sarar. */
export function useForgotPassword() {
  const { language, t } = useLanguage()
  const [submittedEmail, setSubmittedEmail] = useState<string>()
  const [serverError, setServerError] = useState<string>()

  const form = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(schema),
    defaultValues,
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })

  const forgot = useAppMutation<AuthMessageResult, string>({
    mutationFn: authPublicService.forgotPassword,
    onSuccess: (_data, value) => {
      setServerError(undefined)
      setSubmittedEmail(value)
    },
    onError: (error) => {
      setSubmittedEmail(undefined)
      setServerError(resolveAuthErrorMessage(error, language))
    },
  })

  const onValid = (values: ForgotPasswordFormValues) => {
    setServerError(undefined)
    forgot.mutate(values.email)
  }

  return {
    t,
    form,
    submittedEmail,
    serverError,
    isSubmitting: forgot.isPending,
    onSubmit: form.handleSubmit(onValid),
  }
}
