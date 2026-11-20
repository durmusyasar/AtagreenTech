import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query'
import { App } from 'antd'
import { useState } from 'react'
import { invalidate } from '@/core/api/queryClient'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import {
  tenantsQueries,
  tenantsService,
  type CreateTenantRequest,
  type TenantListItem,
  type TenantModuleItemRequest,
} from '@modules/system/api/tenantsService'
import { queryKeys } from '@/services/queryKeys'

const PAGE_SIZE = 20

export function useTenantsManagement() {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const queryClient = useQueryClient()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [createOpen, setCreateOpen] = useState(false)
  const [modulesTenant, setModulesTenant] = useState<TenantListItem | null>(null)

  const listQuery = useQuery({
    ...tenantsQueries.list({ page, pageSize: PAGE_SIZE, search }),
    placeholderData: keepPreviousData,
  })

  const createMutation = useAppMutation({
    mutationFn: (body: CreateTenantRequest) => tenantsService.create(body),
    onSuccess: async (result) => {
      message.success(result.message || t('tenants.created'))
      setCreateOpen(false)
      await invalidate(queryClient, queryKeys.tenants)
    },
  })

  const updateModulesMutation = useAppMutation({
    mutationFn: ({
      tenantId,
      modules,
    }: {
      tenantId: string
      modules: TenantModuleItemRequest[]
    }) => tenantsService.updateModules(tenantId, modules),
    onSuccess: async () => {
      message.success(t('tenants.modulesUpdated'))
      setModulesTenant(null)
      await invalidate(queryClient, queryKeys.tenants)
      await invalidate(queryClient, queryKeys.navigation)
    },
  })

  return {
    t,
    page,
    setPage,
    pageSize: PAGE_SIZE,
    search,
    setSearch,
    listQuery,
    createOpen,
    setCreateOpen,
    modulesTenant,
    setModulesTenant,
    createMutation,
    updateModulesMutation,
  }
}
