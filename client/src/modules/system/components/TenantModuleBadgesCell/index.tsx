import { Flex, Tag, Tooltip, Typography } from 'antd'
import { useLanguage } from '@/core/context/useLanguage'
import type { TenantModuleBadgesCellProps } from '@modules/system/components/TenantModuleBadgesCell/type'

/**
 * Tenant listesi aktif-modül sütunu: opsiyonel rozetler + sayım chip’leri.
 * Süresi dolmuş abonelikler uyarı rengi ile işaretlenir.
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

  return (
    <Tooltip title={summary}>
      <Flex vertical gap={6} className="min-w-0 py-0.5">
        <Flex wrap gap={4} align="center">
          {badges.length === 0 ? (
            <Typography.Text type="secondary">—</Typography.Text>
          ) : (
            badges.map((badge) => (
              <Tag
                key={badge.code}
                className={
                  badge.isExpired
                    ? 'm-0 border-amber-500/40 bg-amber-500/15 text-amber-200'
                    : 'm-0 border-brand/35 bg-brand/15 text-brand'
                }
              >
                {badge.code}
                {badge.isExpired ? ` · ${t('tenants.moduleExpired')}` : ''}
              </Tag>
            ))
          )}
        </Flex>
        <Flex wrap gap={4}>
          <span className="inline-flex items-center rounded-full bg-elevated px-2 py-0.5 text-[11px] text-secondary shadow-[inset_0_0_0_1px_rgb(var(--border-subtle)/0.55)] dark:shadow-[inset_0_0_0_1px_rgb(255_255_255/0.08)]">
            {t('tenants.chipOptional', String(row.optionalActiveModuleCount))}
          </span>
          {row.expiredModuleCount > 0 ? (
            <span className="inline-flex items-center rounded-full bg-amber-500/10 px-2 py-0.5 text-[11px] text-amber-200 shadow-[inset_0_0_0_1px_rgb(245_158_11/0.35)]">
              {t('tenants.chipExpired', String(row.expiredModuleCount))}
            </span>
          ) : null}
          <span className="inline-flex items-center rounded-full bg-transparent px-1.5 py-0.5 text-[11px] text-muted">
            {t('tenants.chipActiveTotal', String(row.activeModuleCount))}
          </span>
        </Flex>
      </Flex>
    </Tooltip>
  )
}
