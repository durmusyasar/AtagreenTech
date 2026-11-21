import { ReloadOutlined, SaveOutlined, SearchOutlined } from '@ant-design/icons'
import { App, Button, Empty, Space, Table } from 'antd'
import { useEffect, useMemo, useRef, useState } from 'react'
import { FloatingInput } from '@/Components/FloatingInput'
import {
  clearListPageState,
  readListPageState,
  writeListPageState,
} from '@/Components/ListPage/listPageStorage'
import { sortOrderForField, toListPageSorter } from '@/Components/ListPage/sorter'
import type {
  ConnectedListPageProps,
  ListPageFilterBase,
  ListPageMeta,
} from '@/Components/ListPage/type'
import { useLanguage } from '@/core/context/useLanguage'

const DEFAULT_PAGE_SIZE = 20
const DEFAULT_PAGE_SIZE_OPTIONS = ['10', '20', '50', '100']

/**
 * Ortak liste kabuğu: FloatingInput arama, filtre temizle/kaydet, refetch,
 * sayfalama + sıralama. Arkas ListPage benzeri sorumluluklar; Atagreen tema/i18n/storage.
 */
export function ConnectedListPage<
  TRecord extends object,
  TFilter extends ListPageFilterBase = ListPageFilterBase,
>({
  storageKey,
  title,
  subtitle,
  brandEyebrow,
  searchLabel,
  searchSubmitLabel,
  clearFiltersLabel,
  saveFiltersLabel,
  filtersSavedLabel,
  refreshLabel,
  listCountLabel,
  emptyTitle,
  emptyDescription,
  emptySearchTitle,
  emptySearchDescription,
  emptyAction,
  primaryActions,
  filterExtra,
  columns,
  dataSource,
  rowKey,
  totalCount,
  loading = false,
  fetching = false,
  pageSizeOptions = DEFAULT_PAGE_SIZE_OPTIONS,
  enableFilterPersistence = true,
  defaultFilter,
  defaultMeta,
  onNeedData,
  onRefetch,
  tableProps,
}: ConnectedListPageProps<TRecord, TFilter>) {
  const { t } = useLanguage()
  const { message } = App.useApp()
  const onNeedDataRef = useRef(onNeedData)

  const initial = resolveInitialState(storageKey, enableFilterPersistence, defaultFilter, defaultMeta)
  const [filter, setFilter] = useState<TFilter>(initial.filter)
  const [meta, setMeta] = useState<ListPageMeta>(initial.meta)
  const [searchDraft, setSearchDraft] = useState(initial.filter.search)

  useEffect(() => {
    onNeedDataRef.current = onNeedData
  }, [onNeedData])

  useEffect(() => {
    onNeedDataRef.current(filter, meta)
  }, [filter, meta])

  const hasActiveFilters = filter.search.trim().length > 0
  const isEmpty = !loading && dataSource.length === 0

  const columnsWithSort = useMemo(
    () =>
      columns.map((column) => {
        const field =
          typeof column.dataIndex === 'string'
            ? column.dataIndex
            : typeof column.key === 'string'
              ? column.key
              : undefined
        if (!field || !column.sorter) return column
        return { ...column, sortOrder: sortOrderForField(meta.sorter, field) }
      }),
    [columns, meta.sorter],
  )

  const applySearch = (raw: string) => {
    const search = raw.trim()
    setSearchDraft(search)
    setFilter((prev) => ({ ...prev, search }))
    setMeta((prev) => ({ ...prev, page: 1 }))
  }

  const clearFilters = () => {
    const cleared = { ...filter, search: '' } as TFilter
    setSearchDraft('')
    setFilter(cleared)
    setMeta((prev) => ({ ...prev, page: 1, sorter: undefined }))
    if (enableFilterPersistence) clearListPageState(storageKey)
  }

  const saveFilters = () => {
    if (!enableFilterPersistence) return
    writeListPageState(storageKey, { filter, meta })
    message.success(filtersSavedLabel)
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <div className="space-y-1.5">
          {brandEyebrow ? (
            <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-brand">
              {brandEyebrow}
            </p>
          ) : null}
          <h1 className="text-2xl font-semibold tracking-tight text-primary sm:text-[1.75rem]">
            {title}
          </h1>
          {subtitle ? (
            <p className="max-w-2xl text-sm leading-relaxed text-secondary">{subtitle}</p>
          ) : null}
        </div>
        <Space wrap>
          {onRefetch ? (
            <Button icon={<ReloadOutlined />} onClick={onRefetch} loading={fetching}>
              {refreshLabel}
            </Button>
          ) : null}
          {primaryActions}
        </Space>
      </div>

      <section className="overflow-hidden rounded-2xl border border-subtle bg-surface dark:border-white/10">
        <div className="flex flex-col gap-3 border-b border-subtle px-4 py-3 dark:border-white/10">
          <div className="flex flex-col gap-3 lg:flex-row lg:items-end lg:justify-between">
            <div className="flex min-w-0 flex-1 flex-col gap-3 sm:flex-row sm:items-end">
              <FloatingInput
                className="w-full max-w-md"
                label={searchLabel}
                value={searchDraft}
                autoClear
                clearLabel={t('common.clear')}
                onChange={(event) => {
                  const next = event.target.value
                  setSearchDraft(next)
                  if (next === '' && filter.search !== '') applySearch('')
                }}
                onKeyDown={(event) => {
                  if (event.key !== 'Enter') return
                  event.preventDefault()
                  applySearch(searchDraft)
                }}
                endAdornment={
                  <button
                    type="button"
                    tabIndex={-1}
                    aria-label={searchSubmitLabel}
                    onClick={() => applySearch(searchDraft)}
                    className="floating-input-clear"
                  >
                    <SearchOutlined className="text-sm" aria-hidden />
                  </button>
                }
              />
              {filterExtra}
            </div>
            <div className="flex flex-wrap items-center gap-2">
              <span className="text-xs text-secondary">{listCountLabel(totalCount)}</span>
              <Button size="small" onClick={clearFilters} disabled={!hasActiveFilters && !meta.sorter}>
                {clearFiltersLabel}
              </Button>
              {enableFilterPersistence ? (
                <Button size="small" icon={<SaveOutlined />} onClick={saveFilters}>
                  {saveFiltersLabel}
                </Button>
              ) : null}
            </div>
          </div>
        </div>

        {isEmpty ? (
          <div className="px-4 py-14">
            <Empty
              image={Empty.PRESENTED_IMAGE_SIMPLE}
              description={
                <div className="space-y-1">
                  <p className="text-sm font-medium text-primary">
                    {hasActiveFilters ? emptySearchTitle : emptyTitle}
                  </p>
                  <p className="text-sm text-secondary">
                    {hasActiveFilters
                      ? emptySearchDescription(filter.search.trim())
                      : emptyDescription}
                  </p>
                </div>
              }
            >
              {hasActiveFilters ? (
                <Button onClick={clearFilters}>{clearFiltersLabel}</Button>
              ) : (
                emptyAction
              )}
            </Empty>
          </div>
        ) : (
          <Table<TRecord>
            {...tableProps}
            rowKey={rowKey}
            loading={loading}
            dataSource={dataSource}
            columns={columnsWithSort}
            pagination={{
              current: meta.page,
              pageSize: meta.pageSize,
              total: totalCount,
              showSizeChanger: true,
              pageSizeOptions,
              className: 'px-4',
            }}
            onChange={(pagination, _filters, sorter) => {
              const nextSorter = toListPageSorter(sorter)
              setMeta({
                page: pagination.current ?? 1,
                pageSize: pagination.pageSize ?? meta.pageSize,
                sorter: nextSorter,
              })
            }}
          />
        )}
      </section>
    </div>
  )
}

function resolveInitialState<TFilter extends ListPageFilterBase>(
  storageKey: string,
  enableFilterPersistence: boolean,
  defaultFilter: TFilter | undefined,
  defaultMeta: Partial<ListPageMeta> | undefined,
): { filter: TFilter; meta: ListPageMeta } {
  const fallbackFilter = (defaultFilter ?? ({ search: '' } as TFilter))
  const fallbackMeta: ListPageMeta = {
    page: defaultMeta?.page ?? 1,
    pageSize: defaultMeta?.pageSize ?? DEFAULT_PAGE_SIZE,
    sorter: defaultMeta?.sorter,
  }

  if (!enableFilterPersistence) {
    return { filter: fallbackFilter, meta: fallbackMeta }
  }

  const stored = readListPageState<TFilter>(storageKey)
  if (!stored) return { filter: fallbackFilter, meta: fallbackMeta }

  return {
    filter: { ...fallbackFilter, ...stored.filter },
    meta: {
      page: stored.meta.page || 1,
      pageSize: stored.meta.pageSize || DEFAULT_PAGE_SIZE,
      sorter: stored.meta.sorter,
    },
  }
}
