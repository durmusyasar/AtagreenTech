import { ApartmentOutlined } from '@ant-design/icons'
import { useMemo } from 'react'
import { ConnectedListPage } from '@/Components/ListPage'
import { StatusTag } from '@/Components/StatusTag'
import type { ListTableColumnsType } from '@/Components/ListPage/type'
import { TenantCreateModal } from '@modules/system/components/TenantCreateModal'
import { TenantModuleBadgesCell } from '@modules/system/components/TenantModuleBadgesCell'
import { TenantModulesDrawer } from '@modules/system/components/TenantModulesDrawer'
import { TenantsFilter } from '@modules/system/components/TenantsFilter'
import {
  tenantsFilterDefaults,
  tenantsFilterSchema,
} from '@modules/system/components/TenantsFilter/schema'
import { useTenantsManagement } from '@modules/system/hooks/useTenantsManagement'
import type { TenantListItem } from '@modules/system/api/tenantsService'

/** Platform tenant listesi — ConnectedListPage + abonelik çekmecesi. */
export function TenantsPage() {
  const {
    t,
    onNeedData,
    listQuery,
    modulesTenant,
    setModulesTenant,
    createMutation,
    updateModulesMutation,
  } = useTenantsManagement()

  const items = listQuery.data?.items ?? []

  const columns = useMemo<ListTableColumnsType<TenantListItem>>(
    () => [
      {
        key: 'name',
        title: t('tenants.form.name'),
        dataIndex: 'name',
        sorter: true,
        render: (name: string) => (
          <span className="text-[13px] font-semibold tracking-tight text-primary">{name}</span>
        ),
        width: 120,
      },
      {
        key: 'slug',
        title: t('tenants.form.slug'),
        dataIndex: 'slug',
        width: 200,
        sorter: true,
        render: (slug: string) => (
          <span className="inline-flex items-center rounded-lg bg-elevated px-2 py-1 font-mono text-[12px] text-secondary shadow-[inset_0_0_0_1px_rgb(var(--border-subtle)/0.55)] dark:shadow-[inset_0_0_0_1px_rgb(255_255_255/0.08)]">
            {slug}
          </span>
        ),
      },
      {
        key: 'isActive',
        title: t('list.status'),
        dataIndex: 'isActive',
        width: 120,
        sorter: true,
        render: (isActive: boolean) => <StatusTag isActive={isActive} />,
      },
      {
        key: 'activeModules',
        title: t('tenants.activeModules'),
        width: 280,
        render: (_value: unknown, row: TenantListItem) => (
          <TenantModuleBadgesCell row={row} />
        ),
      },
      {
        key: 'actions',
        title: t('tenants.subscriptions'),
        width: 148,
        listColumnLocked: true,
        render: (_value: unknown, row: TenantListItem) => (
          <button
            type="button"
            className="list-page-btn list-page-btn--quiet"
            onClick={() => setModulesTenant(row)}
          >
            <ApartmentOutlined aria-hidden />
            {t('tenants.subscriptions')}
          </button>
        ),
      },
    ],
    [setModulesTenant, t],
  )

  return (
    <>
      <ConnectedListPage<TenantListItem>
        storageKey="system/tenants"
        brandEyebrow={t('brand.name')}
        title={t('tenants.title')}
        subtitle={t('tenants.subtitle')}
        clearFiltersLabel={t('listPage.clearFilters')}
        saveFiltersLabel={t('listPage.saveFilters')}
        filtersSavedLabel={t('listPage.filtersSaved')}
        refreshLabel={t('common.refresh')}
        createLabel={t('tenants.create')}
        enableCreate
        listCountLabel={(total) => t('tenants.listCount', String(total))}
        emptyTitle={t('tenants.emptyTitle')}
        emptyDescription={t('tenants.emptyDescription')}
        emptySearchTitle={t('tenants.emptySearchTitle')}
        emptySearchDescription={(search) => t('tenants.emptySearchDescription', search)}
        filterContent={<TenantsFilter />}
        filterSchema={tenantsFilterSchema}
        defaultFilter={tenantsFilterDefaults}
        onNeedData={onNeedData}
        onRefetch={() => void listQuery.refetch()}
        loading={listQuery.isPending}
        fetching={listQuery.isFetching}
        dataSource={items}
        totalCount={listQuery.data?.totalCount ?? 0}
        rowKey="id"
        columns={columns}
        onDetailPage={(detail) => (
          <TenantCreateModal
            open={detail.open && detail.reason === 'create'}
            confirming={createMutation.isPending}
            onClose={detail.onClose}
            onSubmit={(body) => {
              createMutation.mutate(body, {
                onSuccess: () => detail.onSaved(),
              })
            }}
          />
        )}
      />

      <TenantModulesDrawer
        tenant={modulesTenant}
        saving={updateModulesMutation.isPending}
        onClose={() => setModulesTenant(null)}
        onSave={(modules) => {
          if (!modulesTenant) return
          updateModulesMutation.mutate({ tenantId: modulesTenant.id, modules })
        }}
      />
    </>
  )
}
