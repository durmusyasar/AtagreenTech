import { keepPreviousData, useQueryClient } from '@tanstack/react-query'
import { App } from 'antd'
import { useState } from 'react'
import { invalidate } from '@/core/api/queryClient'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { useAppQuery } from '@/core/hooks/useAppQuery'
import { queryKeys } from '@/services/queryKeys'
import { userQueries, userService } from '@/services/userService'
import type { UserItem } from '@/types/user'
import type { StatusFilter } from '@/routes/admin/users/type'

const PAGE_SIZE = 15

/** Kullanıcı listesi filtre / sayfalama / aktivasyon işlemleri. */
export function useUsersManagement() {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const queryClient = useQueryClient()

  const [pageNumber, setPageNumber] = useState(1)
  const [searchTerm, setSearchTerm] = useState<string>()
  const [status, setStatus] = useState<StatusFilter>('all')
  const [inviteOpen, setInviteOpen] = useState(false)
  const [editor, setEditor] = useState<{ open: boolean; user: UserItem | null }>({
    open: false,
    user: null,
  })

  const page = useAppQuery({
    ...userQueries.list({
      pageNumber,
      pageSize: PAGE_SIZE,
      searchTerm,
      isActive: status === 'all' ? undefined : status === 'active',
    }),
    placeholderData: keepPreviousData,
  })

  const refresh = () => invalidate(queryClient, queryKeys.users)

  const toggleActive = useAppMutation({
    mutationFn: (user: UserItem) =>
      user.isActive ? userService.deactivate(user.id) : userService.activate(user.id),
    onSuccess: async (_, user) => {
      message.success(t(user.isActive ? 'users.deactivated' : 'users.activated', user.email))
      await refresh()
    },
  })

  const remove = useAppMutation({
    mutationFn: (user: UserItem) => userService.remove(user.id),
    onSuccess: async (_, user) => {
      message.success(t('users.deleted', user.email))
      await refresh()
    },
  })

  const filter =
    <T,>(set: (value: T) => void) =>
    (value: T) => {
      setPageNumber(1)
      set(value)
    }

  return {
    pageSize: PAGE_SIZE,
    pageNumber,
    setPageNumber,
    searchTerm,
    status,
    inviteOpen,
    setInviteOpen,
    editor,
    setEditor,
    page,
    refresh,
    toggleActive,
    remove,
    filterSearch: filter(setSearchTerm),
    filterStatus: filter(setStatus),
    t,
  }
}
