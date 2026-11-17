import { useCallback, useState, type FormEvent } from 'react'
import { z } from 'zod'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { authPublicService } from '@/services/authPublicService'

export interface ForgotPasswordFieldErrors {
  email?: string
}

/** Şifremi unuttum — FloatingInput + Zod alan doğrulaması. */
export function useForgotPassword() {
  const { t } = useLanguage()
  const [email, setEmail] = useState('')
  const [fieldErrors, setFieldErrors] = useState<ForgotPasswordFieldErrors>({})
  const [submittedEmail, setSubmittedEmail] = useState<string>()

  const forgot = useAppMutation({
    mutationFn: (value: string) => authPublicService.forgotPassword(value),
    onSuccess: (_data, value) => setSubmittedEmail(value),
    onError: (_error, value) => setSubmittedEmail(value),
  })

  const validate = useCallback(() => {
    const schema = z
      .object({
        email: z
          .string()
          .trim()
          .min(1, 'validation.emailRequired')
          .refine((value) => z.string().email().safeParse(value).success, {
            message: 'validation.emailInvalid',
          }),
      })
      .safeParse({ email })

    if (schema.success) {
      setFieldErrors({})
      return schema.data.email
    }

    const next: ForgotPasswordFieldErrors = {}
    for (const issue of schema.error.issues) {
      if (issue.path[0] === 'email' && !next.email) next.email = issue.message
    }
    setFieldErrors(next)
    return null
  }, [email])

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    const validEmail = validate()
    if (!validEmail) return
    forgot.mutate(validEmail)
  }

  return {
    t,
    email,
    setEmail,
    fieldErrors,
    submittedEmail,
    isSubmitting: forgot.isPending,
    handleSubmit,
  }
}
