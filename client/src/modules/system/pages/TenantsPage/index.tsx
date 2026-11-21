import { ApartmentOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { Button, Empty, Input, Space, Table, Tag, Typography } from 'antd'
import { useState } from 'react'
import { StatusTag } from '@/Components/StatusTag'
import { TenantCreateModal } from '@modules/system/components/TenantCreateModal'
import { TenantModuleBadgesCell } from '@modules/system/components/TenantModuleBadgesCell'
import { TenantModulesDrawer } from '@modules/system/components/TenantModulesDrawer'
import { useTenantsManagement } from '@modules/system/hooks/useTenantsManagement'
import type { TenantListItem } from '@modules/system/api/tenantsService'

/** Platform tenant listesi — kart kabuğu, rozet özeti ve abonelik çekmecesi. */
export function TenantsPage() {
  const {
    t,
    page,
    setPage,
    pageSize,
    search,
    setSearch,
    listQuery,
    createOpen,
    setCreateOpen,
    modulesTenant,
    setModulesTenant,
    createMutation,
    updateModulesMutation,
  } = useTenantsManagement()
  const [searchDraft, setSearchDraft] = useState(search)

  const items = listQuery.data?.items ?? []
  const isEmpty = !listQuery.isPending && items.length === 0
  const hasActiveSearch = search.trim().length > 0

  const clearSearch = () => {
    setSearchDraft('')
    setSearch('')
    setPage(1)
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div className="space-y-1.5">
          <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-brand">
            {t('brand.name')}
          </p>
          <h1 className="text-2xl font-semibold tracking-tight text-primary sm:text-[1.75rem]">
            {t('tenants.title')}
          </h1>
          <p className="max-w-2xl text-sm leading-relaxed text-secondary">{t('tenants.subtitle')}</p>
        </div>
        <Space wrap>
          <Button
            icon={<ReloadOutlined />}
            onClick={() => void listQuery.refetch()}
            loading={listQuery.isFetching}
          >
            {t('common.refresh')}
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
            {t('tenants.create')}
          </Button>
        </Space>
      </div>

      <section className="overflow-hidden rounded-2xl border border-subtle bg-surface dark:border-white/10">
        <div className="flex flex-col gap-3 border-b border-subtle px-4 py-3 sm:flex-row sm:items-center sm:justify-between dark:border-white/10">
          <Input.Search
            allowClear
            value={searchDraft}
            placeholder={t('tenants.search')}
            onChange={(event) => setSearchDraft(event.target.value)}
            onSearch={(value) => {
              const next = value.trim()
              setSearchDraft(next)
              setSearch(next)
              setPage(1)
            }}
            onClear={clearSearch}
            className="max-w-md"
          />
          <Typography.Text type="secondary" className="text-xs">
            {t('tenants.listCount', String(listQuery.data?.totalCount ?? 0))}
          </Typography.Text>
        </div>

        {isEmpty ? (
          <div className="px-4 py-14">
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={
                <div className="space-y-1">
                  <p className="text-sm font-medium text-primary">
                    {hasActiveSearch ? t('tenants.emptySearchTitle') : t('tenants.emptyTitle')}
                  </p>
                  <p className="text-sm text-secondary">
                    {hasActiveSearch
                      ? t('tenants.emptySearchDescription', search.trim())
                      : t('tenants.emptyDescription')}
                  </p>
                </div>
              }
            >
              {hasActiveSearch ? (
                <Button onClick={clearSearch}>{t('tenants.clearSearch')}</Button>
              ) : (
                <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreateOpen(true)}>
                  {t('tenants.create')}
                </Button>
              )}
            </Empty>
          </div>
        ) : (
          <Table<TenantListItem>
            rowKey="id"
            loading={listQuery.isPending}
            dataSource={items}
            pagination={{
              current: page,
              pageSize,
              total: listQuery.data?.totalCount ?? 0,
              onChange: setPage,
              showSizeChanger: false,
              className: 'px-4',
            }}
            columns={[
              {
                title: t('tenants.form.name'),
                dataIndex: 'name',
                render: (name: string) => (
                  <span className="font-medium text-primary">{name}</span>
                ),
              },
              {
                title: t('tenants.form.slug'),
                dataIndex: 'slug',
                width: 160,
                render: (slug: string) => (
                  <Tag className="m-0 border-subtle bg-elevated text-secondary">{slug}</Tag>
                ),
              },
              {
                title: t('list.status'),
                dataIndex: 'isActive',
                width: 120,
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
        )}
      </section>

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
    </div>
  )
}
