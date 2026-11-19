import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { resolveAuthFormError } from '@/core/i18n/resolveAuthFormError'
import {
  authPublicService,
  type AcceptInvitePayload,
  type AuthMessageResult,
} from '@/services/authPublicService'
import { defaultValues } from '@/routes/auth/accept-invite/defaultValues'
import { schema, type AcceptInviteFormValues } from '@/routes/auth/accept-invite/schema'

/** Davet kabul formu — Zod + parola politikası; UI `FormProvider` ile sarar. */
export function useAcceptInvite() {
  const { language, t } = useLanguage()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')?.trim() ?? ''
  const [serverError, setServerError] = useState<string>()

  const form = useForm<AcceptInviteFormValues>({
    resolver: zodResolver(schema),
    defaultValues,
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })

  const accept = useAppMutation<AuthMessageResult, AcceptInvitePayload>({
    mutationFn: authPublicService.acceptInvite,
    onSuccess: () => navigate('/login?invited=1', { replace: true }),
    onError: (error) => {
      setServerError(resolveAuthFormError(error, language, t('invite.failed')))
    },
  })

  const onValid = (values: AcceptInviteFormValues) => {
    setServerError(undefined)

    if (!token) {
      setServerError(t('invite.missingToken'))
      return
    }

    accept.mutate({
      token,
      firstName: values.firstName,
      lastName: values.lastName,
      password: values.password,
    })
  }

  return {
    t,
    form,
    token,
    serverError,
    isSubmitting: accept.isPending,
    onSubmit: form.handleSubmit(onValid),
  }
}
