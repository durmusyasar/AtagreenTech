import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query'
import { App } from 'antd'
import { useCallback, useState } from 'react'
import type { ListPageFilterBase, ListPageMeta } from '@/Components/ListPage/type'
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

export interface TenantsListQuery extends ListPageFilterBase {
  page: number
  pageSize: number
  sortBy?: string
  sortDir?: 'asc' | 'desc'
}

/** Tenant listesi + oluştur / modül güncelle mutasyonları. */
export function useTenantsManagement() {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const queryClient = useQueryClient()
  const [listQueryParams, setListQueryParams] = useState<TenantsListQuery>({
    search: '',
    page: 1,
    pageSize: 20,
  })
  const [createOpen, setCreateOpen] = useState(false)
  const [modulesTenant, setModulesTenant] = useState<TenantListItem | null>(null)

  const listQuery = useQuery({
    ...tenantsQueries.list(listQueryParams),
    placeholderData: keepPreviousData,
  })

  const onNeedData = useCallback((filter: ListPageFilterBase, meta: ListPageMeta) => {
    setListQueryParams({
      search: filter.search,
      page: meta.page,
      pageSize: meta.pageSize,
      sortBy: meta.sorter?.field,
      sortDir: meta.sorter?.order === 'descend' ? 'desc' : meta.sorter ? 'asc' : undefined,
    })
  }, [])

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
    listQueryParams,
    onNeedData,
    listQuery,
    createOpen,
    setCreateOpen,
    modulesTenant,
    setModulesTenant,
    createMutation,
    updateModulesMutation,
  }
}
