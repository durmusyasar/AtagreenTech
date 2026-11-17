import { useQueryClient } from '@tanstack/react-query'
import { App } from 'antd'
import { invalidate } from '@/core/api/queryClient'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { useAppQuery } from '@/core/hooks/useAppQuery'
import { queryKeys } from '@/services/queryKeys'
import { accountSecurityQueries, accountSecurityService } from '@/services/accountSecurityService'

/** Aktif oturum listesi + iptal işlemleri. */
export function useActiveSessions() {
  const { language, t } = useLanguage()
  const { message } = App.useApp()
  const queryClient = useQueryClient()
  const sessions = useAppQuery(accountSecurityQueries.sessions())

  const refresh = () => invalidate(queryClient, queryKeys.sessions)

  const revokeOne = useAppMutation({
    mutationFn: (id: string) => accountSecurityService.revokeSession(id),
    onSuccess: async () => {
      message.success(t('security.sessionRevoked'))
      await refresh()
    },
  })

  const revokeOthers = useAppMutation({
    mutationFn: () => accountSecurityService.revokeOtherSessions(),
    onSuccess: async () => {
      message.success(t('security.otherSessionsRevoked'))
      await refresh()
    },
  })

  return { language, t, sessions, revokeOne, revokeOthers }
}
