import { ApartmentOutlined, PlusOutlined } from '@ant-design/icons'
import { Button, Tag } from 'antd'
import { ConnectedListPage } from '@/Components/ListPage'
import { StatusTag } from '@/Components/StatusTag'
import { TenantCreateModal } from '@modules/system/components/TenantCreateModal'
import { TenantModuleBadgesCell } from '@modules/system/components/TenantModuleBadgesCell'
import { TenantModulesDrawer } from '@modules/system/components/TenantModulesDrawer'
import { useTenantsManagement } from '@modules/system/hooks/useTenantsManagement'
import type { TenantListItem } from '@modules/system/api/tenantsService'

/** Platform tenant listesi — ConnectedListPage + abonelik çekmecesi. */
export function TenantsPage() {
  const {
    t,
    onNeedData,
    listQuery,
    createOpen,
    setCreateOpen,
    modulesTenant,
    setModulesTenant,
    createMutation,
    updateModulesMutation,
  } = useTenantsManagement()

  const items = listQuery.data?.items ?? []

  return (
    <>
      <ConnectedListPage<TenantListItem>
        storageKey="system/tenants"
        brandEyebrow={t('brand.name')}
        title={t('tenants.title')}
        subtitle={t('tenants.subtitle')}
        searchLabel={t('tenants.search')}
        searchSubmitLabel={t('tenants.searchSubmit')}
        clearFiltersLabel={t('listPage.clearFilters')}
        saveFiltersLabel={t('listPage.saveFilters')}
        filtersSavedLabel={t('listPage.filtersSaved')}
        refreshLabel={t('common.refresh')}
        listCountLabel={(total) => t('tenants.listCount', String(total))}
        emptyTitle={t('tenants.emptyTitle')}
        emptyDescription={t('tenants.emptyDescription')}
        emptySearchTitle={t('tenants.emptySearchTitle')}
        emptySearchDescription={(search) => t('tenants.emptySearchDescription', search)}
        emptyAction={
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
            {t('tenants.create')}
          </Button>
        }
        primaryActions={
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
            {t('tenants.create')}
          </Button>
        }
        onNeedData={onNeedData}
        onRefetch={() => void listQuery.refetch()}
        loading={listQuery.isPending}
        fetching={listQuery.isFetching}
        dataSource={items}
        totalCount={listQuery.data?.totalCount ?? 0}
        rowKey="id"
        columns={[
          {
            title: t('tenants.form.name'),
            dataIndex: 'name',
            sorter: true,
            render: (name: string) => <span className="font-medium text-primary">{name}</span>,
          },
          {
            title: t('tenants.form.slug'),
            dataIndex: 'slug',
            width: 160,
            sorter: true,
            render: (slug: string) => (
              <Tag className="m-0 border-subtle bg-elevated text-secondary">{slug}</Tag>
            ),
          },
          {
            title: t('list.status'),
            dataIndex: 'isActive',
            width: 120,
            sorter: true,
            render: (isActive: boolean) => <StatusTag isActive={isActive} />,
          },
          {
            title: t('tenants.activeModules'),
            key: 'activeModules',
            width: 280,
            render: (_, row) => <TenantModuleBadgesCell row={row} />,
          },
          {
            title: t('tenants.subscriptions'),
            key: 'actions',
            width: 150,
            render: (_, row) => (
              <Button
                size="small"
                icon={<ApartmentOutlined />}
                onClick={() => setModulesTenant(row)}
              >
                {t('tenants.subscriptions')}
              </Button>
            ),
          },
        ]}
      />

      <TenantCreateModal
        open={createOpen}
        confirming={createMutation.isPending}
        onClose={() => setCreateOpen(false)}
        onSubmit={(body) => createMutation.mutate(body)}
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
