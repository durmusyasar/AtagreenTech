import { ApartmentOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { Button, Flex, Input, Space, Table, Tag, Typography } from 'antd'
import { StatusTag } from '@/Components/StatusTag'
import { TenantCreateModal } from '@modules/system/components/TenantCreateModal'
import { TenantModulesDrawer } from '@modules/system/components/TenantModulesDrawer'
import { useTenantsManagement } from '@modules/system/hooks/useTenantsManagement'
import type { TenantListItem } from '@modules/system/api/tenantsService'

export function TenantsPage() {
  const {
    t,
    page,
    setPage,
    pageSize,
    setSearch,
    listQuery,
    createOpen,
    setCreateOpen,
    modulesTenant,
    setModulesTenant,
    createMutation,
    updateModulesMutation,
  } = useTenantsManagement()

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Flex justify="space-between" align="center" wrap gap={16}>
        <div>
          <Typography.Title level={3} style={{ margin: 0 }}>
            {t('tenants.title')}
          </Typography.Title>
          <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
            {t('tenants.subtitle')}
          </Typography.Paragraph>
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
      </Flex>

      <Input.Search
        allowClear
        placeholder={t('tenants.search')}
        onSearch={(value) => {
          setSearch(value.trim())
          setPage(1)
        }}
        style={{ maxWidth: 360 }}
      />

      <Table<TenantListItem>
        rowKey="id"
        loading={listQuery.isPending}
        dataSource={listQuery.data?.items}
        pagination={{
          current: page,
          pageSize,
          total: listQuery.data?.totalCount ?? 0,
          onChange: setPage,
          showSizeChanger: false,
        }}
        columns={[
          {
            title: t('tenants.form.name'),
            dataIndex: 'name',
          },
          {
            title: t('tenants.form.slug'),
            dataIndex: 'slug',
            width: 160,
            render: (slug: string) => <Tag>{slug}</Tag>,
          },
          {
            title: t('list.status'),
            dataIndex: 'isActive',
            width: 120,
            render: (isActive: boolean) => <StatusTag isActive={isActive} />,
          },
          {
            title: t('tenants.enabledModules'),
            dataIndex: 'enabledModuleCount',
            width: 140,
          },
          {
            title: t('tenants.modules'),
            key: 'actions',
            width: 160,
            render: (_, row) => (
              <Button
                type="link"
                icon={<ApartmentOutlined />}
                onClick={() => setModulesTenant(row)}
              >
                {t('tenants.modules')}
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
    </Space>
  )
}
