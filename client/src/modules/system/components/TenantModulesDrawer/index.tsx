import { useQuery } from '@tanstack/react-query'
import { Button, DatePicker, Drawer, Flex, Space, Switch, Table, Tag, Typography } from 'antd'
import dayjs from 'dayjs'
import { useEffect, useMemo, useState } from 'react'
import { useLanguage } from '@/core/context/useLanguage'
import {
  tenantsQueries,
  type TenantModuleItem,
  type TenantModuleItemRequest,
} from '@modules/system/api/tenantsService'
import type {
  TenantModuleDraftRow,
  TenantModulesDrawerProps,
} from '@modules/system/components/TenantModulesDrawer/type'

/** Tenant modül abonelik çekmecesi — core kilidi, süresiz / süresi dolmuş uyarıları. */
export function TenantModulesDrawer({ tenant, saving, onClose, onSave }: TenantModulesDrawerProps) {
  const { t } = useLanguage()
  const [draft, setDraft] = useState<TenantModuleDraftRow[]>([])

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

  const expiredCount = draft.filter(isDraftExpired).length

  return (
    <Drawer
      title={tenant ? t('tenants.modulesTitle', tenant.name) : t('tenants.modules')}
      open={Boolean(tenant)}
      onClose={onClose}
      width={600}
      destroyOnHidden
      styles={{ body: { paddingTop: 16 } }}
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
      <div className="mb-4 space-y-3">
        <Typography.Paragraph type="secondary" className="!mb-0">
          {t('tenants.modulesHint')}
        </Typography.Paragraph>
        {expiredCount > 0 ? (
          <div
            role="status"
            className="rounded-xl border border-amber-500/35 bg-amber-500/10 px-3 py-2 text-sm text-amber-100"
          >
            {t('tenants.drawerExpiredBanner', String(expiredCount))}
          </div>
        ) : null}
      </div>

      <Table<TenantModuleDraftRow>
        rowKey="moduleId"
        size="middle"
        loading={modulesQuery.isFetching}
        dataSource={draft}
        pagination={false}
        rowClassName={(row) => (isDraftExpired(row) ? 'bg-amber-500/[0.06]' : '')}
        columns={[
          {
            title: t('tenants.form.module'),
            key: 'module',
            render: (_, row) => (
              <Flex vertical gap={4}>
                <span className="font-medium text-primary">{row.name}</span>
                <Flex gap={4} wrap>
                  <Tag className="m-0 w-fit border-subtle bg-elevated text-secondary">{row.code}</Tag>
                  {row.isCore ? (
                    <Tag className="m-0 w-fit border-brand/30 bg-brand/10 text-brand">
                      {t('tenants.moduleCore')}
                    </Tag>
                  ) : null}
                  {isDraftExpired(row) ? (
                    <Tag color="warning" className="m-0 w-fit">
                      {t('tenants.moduleExpired')}
                    </Tag>
                  ) : null}
                </Flex>
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
            width: 180,
            render: (_, row) => {
              if (row.isCore) {
                return <Typography.Text type="secondary">—</Typography.Text>
              }
              if (!row.isEnabled) {
                return <Typography.Text type="secondary">—</Typography.Text>
              }
              if (!row.expiresAtUtc) {
                return (
                  <Flex vertical gap={6}>
                    <Typography.Text type="secondary" className="text-xs">
                      {t('tenants.moduleUnlimited')}
                    </Typography.Text>
                    <DatePicker
                      allowClear
                      value={row.expiresAtUtc}
                      style={{ width: '100%' }}
                      status={isDraftExpired(row) ? 'warning' : undefined}
                      onChange={(value) =>
                        setDraft((prev) =>
                          prev.map((item) =>
                            item.moduleId === row.moduleId ? { ...item, expiresAtUtc: value } : item,
                          ),
                        )
                      }
                    />
                  </Flex>
                )
              }
              return (
                <DatePicker
                  allowClear
                  value={row.expiresAtUtc}
                  style={{ width: '100%' }}
                  status={isDraftExpired(row) ? 'warning' : undefined}
                  onChange={(value) =>
                    setDraft((prev) =>
                      prev.map((item) =>
                        item.moduleId === row.moduleId ? { ...item, expiresAtUtc: value } : item,
                      ),
                    )
                  }
                />
              )
            },
          },
        ]}
      />
    </Drawer>
  )
}

function isDraftExpired(row: TenantModuleDraftRow): boolean {
  if (row.isCore || !row.isEnabled || !row.expiresAtUtc) return false
  return !row.expiresAtUtc.isAfter(dayjs())
}

function toDraft(item: TenantModuleItem): TenantModuleDraftRow {
  return {
    moduleId: item.moduleId,
    code: item.code,
    name: item.name,
    isCore: item.isCore,
    isEnabled: item.isEnabled,
    expiresAtUtc: item.expiresAtUtc ? dayjs(item.expiresAtUtc) : null,
  }
}

function fromDraft(row: TenantModuleDraftRow): TenantModuleItemRequest {
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
