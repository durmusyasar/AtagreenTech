import { useCallback, useState, type FormEvent } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { z } from 'zod'
import { ApiError } from '@/core/api/apiError'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { checkPasswordStrength } from '@/core/passwordPolicy'
import { authPublicService } from '@/services/authPublicService'

export interface AcceptInviteFieldErrors {
  firstName?: string
  lastName?: string
  password?: string
  confirmPassword?: string
}

/** Davet kabul — FloatingInput + Zod + parola politikası. */
export function useAcceptInvite() {
  const { t } = useLanguage()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')?.trim() ?? ''

  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [fieldErrors, setFieldErrors] = useState<AcceptInviteFieldErrors>({})
  const [serverError, setServerError] = useState<string>()

  const accept = useAppMutation({
    mutationFn: authPublicService.acceptInvite,
    onSuccess: () => navigate('/login?invited=1', { replace: true }),
    onError: (error) => {
      setServerError(
        error instanceof ApiError && error.message ? error.message : t('invite.failed'),
      )
    },
  })

  const validate = useCallback(() => {
    const schema = z
      .object({
        firstName: z.string().trim().min(1, 'invite.firstNameRequired'),
        lastName: z.string().trim().min(1, 'invite.lastNameRequired'),
        password: z.string().min(1, 'validation.passwordRequired'),
        confirmPassword: z.string().min(1, 'invite.confirmPasswordRequired'),
      })
      .superRefine((data, ctx) => {
        if (data.confirmPassword && data.password !== data.confirmPassword) {
          ctx.addIssue({
            code: 'custom',
            path: ['confirmPassword'],
            message: 'password.mismatch',
          })
        }
      })
      .safeParse({ firstName, lastName, password, confirmPassword })

    if (!schema.success) {
      const next: AcceptInviteFieldErrors = {}
      for (const issue of schema.error.issues) {
        const key = issue.path[0]
        if (
          (key === 'firstName' ||
            key === 'lastName' ||
            key === 'password' ||
            key === 'confirmPassword') &&
          !next[key]
        ) {
          next[key] = issue.message
        }
      }
      setFieldErrors(next)
      return null
    }

    if (!checkPasswordStrength(schema.data.password).isValid) {
      setFieldErrors({ password: 'password.policyFailed' })
      return null
    }

    setFieldErrors({})
    return schema.data
  }, [firstName, lastName, password, confirmPassword])

  const handleSubmit = (event: FormEvent) => {
    event.preventDefault()
    setServerError(undefined)

    if (!token) {
      setServerError(t('invite.missingToken'))
      return
    }

    const values = validate()
    if (!values) return

    accept.mutate({
      token,
      firstName: values.firstName,
      lastName: values.lastName,
      password: values.password,
    })
  }

  return {
    t,
    token,
    firstName,
    setFirstName,
    lastName,
    setLastName,
    password,
    setPassword,
    confirmPassword,
    setConfirmPassword,
    fieldErrors,
    serverError,
    isSubmitting: accept.isPending,
    handleSubmit,
  }
}
