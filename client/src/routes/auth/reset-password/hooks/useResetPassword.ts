import { useCallback, useState, type FormEvent } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { z } from 'zod'
import { ApiError } from '@/core/api/apiError'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { checkPasswordStrength } from '@/core/passwordPolicy'
import { authPublicService } from '@/services/authPublicService'

export interface ResetPasswordFieldErrors {
  newPassword?: string
  confirmPassword?: string
}

/** Yeni şifre belirleme — FloatingInput + Zod + parola politikası. */
export function useResetPassword() {
  const { t } = useLanguage()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')?.trim() ?? ''

  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [fieldErrors, setFieldErrors] = useState<ResetPasswordFieldErrors>({})
  const [serverError, setServerError] = useState<string>()

  const reset = useAppMutation({
    mutationFn: authPublicService.resetPassword,
    onSuccess: () => navigate('/login?reset=1', { replace: true }),
    onError: (error) => {
      setServerError(
        error instanceof ApiError && error.message ? error.message : t('reset.failed'),
      )
    },
  })

  const validate = useCallback(() => {
    const schema = z
      .object({
        newPassword: z.string().min(1, 'validation.passwordRequired'),
        confirmPassword: z.string().min(1, 'invite.confirmPasswordRequired'),
      })
      .superRefine((data, ctx) => {
        if (data.confirmPassword && data.newPassword !== data.confirmPassword) {
          ctx.addIssue({
            code: 'custom',
            path: ['confirmPassword'],
            message: 'password.mismatch',
          })
        }
      })
      .safeParse({ newPassword, confirmPassword })

    if (!schema.success) {
      const next: ResetPasswordFieldErrors = {}
      for (const issue of schema.error.issues) {
        const key = issue.path[0]
        if ((key === 'newPassword' || key === 'confirmPassword') && !next[key]) {
          next[key] = issue.message
        }
      }
      setFieldErrors(next)
      return null
    }

    if (!checkPasswordStrength(schema.data.newPassword).isValid) {
      setFieldErrors({ newPassword: 'password.policyFailed' })
      return null
    }

    setFieldErrors({})
    return schema.data
  }, [newPassword, confirmPassword])

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    setServerError(undefined)

    if (!token) {
      setServerError(t('reset.missingToken'))
      return
    }

    const values = validate()
    if (!values) return

    reset.mutate({ token, newPassword: values.newPassword })
  }

  return {
    t,
    token,
    newPassword,
    setNewPassword,
    confirmPassword,
    setConfirmPassword,
    fieldErrors,
    serverError,
    isSubmitting: reset.isPending,
    handleSubmit,
  }
}
