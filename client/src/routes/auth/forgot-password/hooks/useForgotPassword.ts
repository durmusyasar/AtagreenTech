import { useState } from 'react'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { authPublicService } from '@/services/authPublicService'

/** Şifremi unuttum formu state + mutation. */
export function useForgotPassword() {
  const { t } = useLanguage()
  const [submittedEmail, setSubmittedEmail] = useState<string>()

  const forgot = useAppMutation({
    mutationFn: (email: string) => authPublicService.forgotPassword(email),
    onSuccess: (_data, email) => setSubmittedEmail(email),
    onError: (_error, email) => setSubmittedEmail(email),
  })

  return { t, submittedEmail, forgot }
}
