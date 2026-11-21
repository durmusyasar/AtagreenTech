import { Flex, Tag, Tooltip, Typography } from 'antd'
import { useLanguage } from '@/core/context/useLanguage'
import type { TenantModuleBadgesCellProps } from '@modules/system/components/TenantModuleBadgesCell/type'

/**
 * Tenant listesi aktif-modül sütunu: opsiyonel rozetler + sayım tooltip.
 * Süresi dolmuş abonelikler uyarı rengi (`warning`) ile işaretlenir.
 */
export function TenantModuleBadgesCell({ row }: TenantModuleBadgesCellProps) {
  const { t } = useLanguage()
  const badges = row.activeModuleBadges ?? []

  const summary = t(
    'tenants.modulesSummary',
    String(row.activeModuleCount),
    String(row.optionalActiveModuleCount),
    String(row.expiredModuleCount),
  )

  if (badges.length === 0) {
    return (
      <Tooltip title={summary}>
        <Typography.Text type="secondary">—</Typography.Text>
      </Tooltip>
    )
  }

  return (
    <Tooltip title={summary}>
      <Flex wrap gap={4}>
        {badges.map((badge) => (
          <Tag key={badge.code} color={badge.isExpired ? 'warning' : undefined}>
            {badge.code}
            {badge.isExpired ? ` · ${t('tenants.moduleExpired')}` : ''}
          </Tag>
        ))}
      </Flex>
    </Tooltip>
  )
}
