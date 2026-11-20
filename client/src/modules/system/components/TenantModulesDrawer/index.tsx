import { useQuery } from '@tanstack/react-query'
import { Button, DatePicker, Drawer, Flex, Space, Switch, Table, Tag, Typography } from 'antd'
import dayjs, { type Dayjs } from 'dayjs'
import { useEffect, useMemo, useState } from 'react'
import { useLanguage } from '@/core/context/useLanguage'
import {
  tenantsQueries,
  type TenantListItem,
  type TenantModuleItem,
  type TenantModuleItemRequest,
} from '@modules/system/api/tenantsService'

interface TenantModulesDrawerProps {
  tenant: TenantListItem | null
  saving: boolean
  onClose: () => void
  onSave: (modules: TenantModuleItemRequest[]) => void
}

interface DraftRow {
  moduleId: string
  code: string
  name: string
  isCore: boolean
  isEnabled: boolean
  expiresAtUtc: Dayjs | null
}

export function TenantModulesDrawer({ tenant, saving, onClose, onSave }: TenantModulesDrawerProps) {
  const { t } = useLanguage()
  const [draft, setDraft] = useState<DraftRow[]>([])

  const modulesQuery = useQuery({
    ...tenantsQueries.modules(tenant?.id ?? ''),
    enabled: Boolean(tenant?.id),
  })

  useEffect(() => {
    if (!modulesQuery.data) return
    setDraft(modulesQuery.data.map(toDraft))
  }, [modulesQuery.data])

  const dirty = useMemo(() => {
    if (!modulesQuery.data) return false
    return JSON.stringify(draft.map(fromDraft)) !== JSON.stringify(modulesQuery.data.map(toRequest))
  }, [draft, modulesQuery.data])

  return (
    <Drawer
      title={tenant ? t('tenants.modulesTitle', tenant.name) : t('tenants.modules')}
      open={Boolean(tenant)}
      onClose={onClose}
      width={560}
      destroyOnHidden
      extra={
        <Space>
          <Button onClick={onClose}>{t('common.cancel')}</Button>
          <Button
            type="primary"
            disabled={!dirty}
            loading={saving}
            onClick={() => onSave(draft.map(fromDraft))}
          >
            {t('common.save')}
          </Button>
        </Space>
      }
    >
      <Typography.Paragraph type="secondary">{t('tenants.modulesHint')}</Typography.Paragraph>
      <Table<DraftRow>
        rowKey="moduleId"
        size="small"
        loading={modulesQuery.isFetching}
        dataSource={draft}
        pagination={false}
        columns={[
          {
            title: t('tenants.form.module'),
            key: 'module',
            render: (_, row) => (
              <Flex vertical gap={2}>
                <span>{row.name}</span>
                <Tag style={{ width: 'fit-content' }}>{row.code}</Tag>
              </Flex>
            ),
          },
          {
            title: t('tenants.form.enabled'),
            key: 'enabled',
            width: 100,
            render: (_, row) => (
              <Switch
                checked={row.isEnabled}
                disabled={row.isCore}
                onChange={(checked) =>
                  setDraft((prev) =>
                    prev.map((item) =>
                      item.moduleId === row.moduleId ? { ...item, isEnabled: checked } : item,
                    ),
                  )
                }
              />
            ),
          },
          {
            title: t('tenants.form.expires'),
            key: 'expires',
            width: 160,
            render: (_, row) =>
              row.isCore ? (
                <Typography.Text type="secondary">—</Typography.Text>
              ) : (
                <DatePicker
                  allowClear
                  value={row.expiresAtUtc}
                  disabled={!row.isEnabled}
                  style={{ width: '100%' }}
                  onChange={(value) =>
                    setDraft((prev) =>
                      prev.map((item) =>
                        item.moduleId === row.moduleId ? { ...item, expiresAtUtc: value } : item,
                      ),
                    )
                  }
                />
              ),
          },
        ]}
      />
    </Drawer>
  )
}

function toDraft(item: TenantModuleItem): DraftRow {
  return {
    moduleId: item.moduleId,
    code: item.code,
    name: item.name,
    isCore: item.isCore,
    isEnabled: item.isEnabled,
    expiresAtUtc: item.expiresAtUtc ? dayjs(item.expiresAtUtc) : null,
  }
}

function fromDraft(row: DraftRow): TenantModuleItemRequest {
  return {
    moduleId: row.moduleId,
    isEnabled: row.isEnabled,
    expiresAtUtc: row.expiresAtUtc ? row.expiresAtUtc.toISOString() : null,
  }
}

function toRequest(item: TenantModuleItem): TenantModuleItemRequest {
  return {
    moduleId: item.moduleId,
    isEnabled: item.isEnabled,
    expiresAtUtc: item.expiresAtUtc,
  }
}
