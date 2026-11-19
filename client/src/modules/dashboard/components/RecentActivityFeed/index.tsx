import { useLanguage } from '@/core/context/useLanguage'
import type { RecentActivityFeedProps } from '@modules/dashboard/components/RecentActivityFeed/type'

export function RecentActivityFeed({ items, isLoading }: RecentActivityFeedProps) {
  const { language, t } = useLanguage()

  return (
    <div className="space-y-4 rounded-2xl border border-subtle bg-surface p-5 dark:border-white/10">
      <h2 className="text-base font-semibold text-primary">{t('dashboard.activity.title')}</h2>

      {isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 5 }).map((_, index) => (
            <div key={index} className="h-12 animate-pulse rounded-xl bg-elevated/80" />
          ))}
        </div>
      ) : items.length === 0 ? (
        <p className="py-10 text-center text-sm text-muted">{t('dashboard.empty')}</p>
      ) : (
        <ul className="divide-y divide-subtle/70 dark:divide-white/10">
          {items.map((item) => (
            <li key={item.id} className="flex flex-col gap-1 py-3 first:pt-0 last:pb-0 sm:flex-row sm:items-center sm:justify-between">
              <div className="min-w-0">
                <p className="truncate text-sm font-medium text-primary">
                  {t(item.descriptionKey, ...(item.descriptionParams ?? []))}
                </p>
                <p className="truncate text-xs text-muted">{item.actorEmail}</p>
              </div>
              <time className="shrink-0 text-xs text-secondary" dateTime={item.occurredAtUtc}>
                {new Date(item.occurredAtUtc).toLocaleString(language)}
              </time>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
