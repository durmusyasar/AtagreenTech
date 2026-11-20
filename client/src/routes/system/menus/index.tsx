import { EditOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { keepPreviousData, useQuery, useQueryClient } from '@tanstack/react-query'
import { App, Button, Flex, Space, Switch, Table, Tag, Typography } from 'antd'
import { useMemo, useState } from 'react'
import { StatusTag } from '@/Components/StatusTag'
import { invalidate } from '@/core/api/queryClient'
import { useLanguage } from '@/core/context/useLanguage'
import { useAppMutation } from '@/core/hooks/useAppMutation'
import { queryKeys } from '@/services/queryKeys'
import {
  systemMenusQueries,
  systemMenusService,
  type SystemMenuListItem,
} from '@/services/systemMenusService'
import { resolveNavTitle } from '@/utils/resolveNavTitle'
import { MenuFormDrawer } from '@/routes/system/menus/MenuFormDrawer'

export function SystemMenusPage() {
  const { language, t } = useLanguage()
  const { message } = App.useApp()
  const queryClient = useQueryClient()
  const [editor, setEditor] = useState<{ open: boolean; menu: SystemMenuListItem | null }>({
    open: false,
    menu: null,
  })

  const menusQuery = useQuery({
    ...systemMenusQueries.menus(),
    placeholderData: keepPreviousData,
  })

  const patch = useAppMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      systemMenusService.patchMenu(id, { isActive }),
    onSuccess: async () => {
      message.success(t('menus.updated'))
      await invalidate(queryClient, queryKeys.systemMenus)
      await invalidate(queryClient, queryKeys.navigation)
    },
  })

  const rows = useMemo(() => {
    const list = menusQuery.data ?? []
    return [...list].sort((a, b) => {
      if (a.moduleCode !== b.moduleCode) return a.moduleCode.localeCompare(b.moduleCode)
      return a.displayOrder - b.displayOrder || a.code.localeCompare(b.code)
    })
  }, [menusQuery.data])

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Flex justify="space-between" align="center" wrap gap={16}>
        <div>
          <Typography.Title level={3} style={{ margin: 0 }}>
            {t('menus.title')}
          </Typography.Title>
          <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
            {t('menus.subtitle')}
          </Typography.Paragraph>
        </div>
        <Space wrap>
          <Button icon={<ReloadOutlined />} onClick={() => menusQuery.refetch()}>
            {t('common.refresh')}
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setEditor({ open: true, menu: null })}>
            {t('menus.create')}
          </Button>
        </Space>
      </Flex>

      <Table<SystemMenuListItem>
        rowKey="id"
        loading={menusQuery.isFetching}
        dataSource={rows}
        pagination={false}
        columns={[
          {
            title: t('menus.form.module'),
            dataIndex: 'moduleCode',
            width: 120,
            render: (code: string) => <Tag>{code}</Tag>,
          },
          { title: t('list.code'), dataIndex: 'code', width: 180 },
          {
            title: t('list.name'),
            key: 'name',
            render: (_, row) => resolveNavTitle(row.translations, row.code, language),
          },
          { title: t('menus.form.route'), dataIndex: 'routePath', width: 180 },
          {
            title: t('menus.form.displayOrder'),
            dataIndex: 'displayOrder',
            width: 90,
          },
          {
            title: t('list.status'),
            dataIndex: 'isActive',
            width: 140,
            render: (isActive: boolean, row) => (
              <Space>
                <StatusTag isActive={isActive} />
                <Switch
                  size="small"
                  checked={isActive}
                  loading={patch.isPending}
                  onChange={(checked) => patch.mutate({ id: row.id, isActive: checked })}
                />
              </Space>
            ),
          },
          {
            key: 'actions',
            width: 100,
            align: 'right',
            render: (_, row) => (
              <Button
                type="link"
                icon={<EditOutlined />}
                onClick={() => setEditor({ open: true, menu: row })}
              >
                {t('common.edit')}
              </Button>
            ),
          },
        ]}
      />

      <MenuFormDrawer
        open={editor.open}
        menu={editor.menu}
        onClose={() => setEditor((current) => ({ ...current, open: false }))}
        onSaved={() => setEditor((current) => ({ ...current, open: false }))}
      />
    </Space>
  )
}
