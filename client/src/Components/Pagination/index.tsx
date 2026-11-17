import { useTranslation } from 'react-i18next'
import type { PaginationProps } from '@/Components/Pagination/type'

/** Liste altı sayfalama çubuğu (`common` namespace). */
export function Pagination({ page, pageSize, totalCount, onPageChange }: PaginationProps) {
  const { t } = useTranslation('common')
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize))

  return (
    <div className="flex items-center justify-between border-t border-slate-200 px-4 py-3 text-xs text-slate-500 dark:border-slate-800 dark:text-slate-400">
      <span>{t('pagination.total', { count: totalCount })}</span>
      <div className="flex items-center gap-2">
        <button
          type="button"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
          className="rounded bg-slate-100 px-2.5 py-1 disabled:opacity-40 dark:bg-slate-800"
        >
          {t('actions.previous')}
        </button>
        <span>{t('pagination.pageOf', { page, totalPages })}</span>
        <button
          type="button"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
          className="rounded bg-slate-100 px-2.5 py-1 disabled:opacity-40 dark:bg-slate-800"
        >
          {t('actions.next')}
        </button>
      </div>
    </div>
  )
}
