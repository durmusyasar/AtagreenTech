import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { ApiError } from '@/core/api/apiError'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { authPublicService } from '@/services/authPublicService'
import type { AcceptInviteFormValues } from '@/routes/auth/accept-invite/type'
import { acceptInviteSchema } from '@/routes/auth/accept-invite/validation/acceptInviteSchema'

const DEFAULTS: AcceptInviteFormValues = {
  firstName: '',
  lastName: '',
  password: '',
  confirmPassword: '',
}

/** Davet kabul — FormProvider + Zod + parola politikası. */
export function useAcceptInvite() {
  const { t } = useLanguage()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')?.trim() ?? ''
  const [serverError, setServerError] = useState<string>()

  const form = useForm<AcceptInviteFormValues>({
    resolver: zodResolver(acceptInviteSchema),
    defaultValues: DEFAULTS,
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })

  const accept = useAppMutation({
    mutationFn: authPublicService.acceptInvite,
    onSuccess: () => navigate('/login?invited=1', { replace: true }),
    onError: (error) => {
      setServerError(
        error instanceof ApiError && error.message ? error.message : t('invite.failed'),
      )
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
