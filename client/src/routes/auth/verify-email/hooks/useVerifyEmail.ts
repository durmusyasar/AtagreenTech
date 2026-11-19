import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useSearchParams } from 'react-router-dom'
import type { AuthPageBanner } from '@/Components/AuthPageShell/type'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { resolveAuthFormError } from '@/core/i18n/resolveAuthFormError'
import { authPublicService, type AuthMessageResult } from '@/services/authPublicService'
import { defaultValues } from '@/routes/auth/verify-email/defaultValues'
import { schema, type VerifyEmailFormValues } from '@/routes/auth/verify-email/schema'
import type { VerifyStatus } from '@/routes/auth/verify-email/type'

/** E-posta doğrulama + resend formu; UI `FormProvider` ile sarar. */
export function useVerifyEmail() {
  const { language, t } = useLanguage()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')?.trim() ?? ''
  const [status, setStatus] = useState<VerifyStatus>(token ? 'VERIFYING' : 'RESEND')
  const [banner, setBanner] = useState<AuthPageBanner>()

  const form = useForm<VerifyEmailFormValues>({
    resolver: zodResolver(schema),
    defaultValues,
    mode: 'onBlur',
    reValidateMode: 'onChange',
  })

  const verify = useAppMutation<AuthMessageResult, string>({
    mutationFn: authPublicService.verifyEmail,
  })

  const resend = useAppMutation<AuthMessageResult, string>({
    mutationFn: authPublicService.resendVerification,
    onSuccess: () => {
      setBanner({ tone: 'info', message: t('verify.resendSent') })
    },
    onError: (error) => {
      setBanner({
        tone: 'error',
        message: resolveAuthFormError(error, language, t('verify.failed')),
      })
    },
  })

  useEffect(() => {
    if (!token) return

    let cancelled = false
    setStatus('VERIFYING')
    setBanner(undefined)

    void verify
      .mutateAsync(token)
      .then(() => {
        if (cancelled) return
        setStatus('SUCCESS')
        setBanner(undefined)
      })
      .catch((error: unknown) => {
        if (cancelled) return
        setStatus('ERROR')
        setBanner({
          tone: 'error',
          message: resolveAuthFormError(error, language, t('verify.failed')),
        })
      })

    return () => {
      cancelled = true
    }
    // Token değişince yeniden doğrula; dil değişiminde tekrar istek atılmaz.
    // eslint-disable-next-line react-hooks/exhaustive-deps -- mutateAsync / language / t kasıtlı
  }, [token])

  const onValid = (values: VerifyEmailFormValues) => {
    setBanner(undefined)
    resend.mutate(values.email)
  }

  return {
    t,
    form,
    status,
    banner,
    isResending: resend.isPending,
    onSubmit: form.handleSubmit(onValid),
  }
}
