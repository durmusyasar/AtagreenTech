import {
  MoreOutlined,
  PlusOutlined,
  ReloadOutlined,
  SaveOutlined,
  TableOutlined,
} from '@ant-design/icons'
import { zodResolver } from '@hookform/resolvers/zod'
import { App, Dropdown, Empty, Table } from 'antd'
import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import { FormProvider, useForm, type DefaultValues, type Resolver } from 'react-hook-form'
import type { ZodType } from 'zod'
import { DefaultListSearchFilter } from '@/Components/ListPage/DefaultListSearchFilter'
import {
  applyColumnPreferences,
  buildDefaultColumnPreferences,
  mergeColumnPreferences,
} from '@/Components/ListPage/columnPreferences'
import {
  clearColumnPreferences,
  readColumnPreferences,
  writeColumnPreferences,
} from '@/Components/ListPage/columnPreferencesStorage'
import {
  defaultListFilterSchema,
  defaultListFilterValues,
} from '@/Components/ListPage/defaultFilterSchema'
import { ListColumnSettingsDrawer } from '@/Components/ListPage/ListColumnSettingsDrawer'
import { ListPageCommandsProvider } from '@/Components/ListPage/ListPageCommandsContext'
import {
  clearListPageState,
  readListPageState,
  writeListPageState,
} from '@/Components/ListPage/listPageStorage'
import { sortOrderForField, toListPageSorter } from '@/Components/ListPage/sorter'
import type {
  ConnectedListPageProps,
  ListColumnPreference,
  ListDetailPageProps,
  ListDetailReason,
  ListPageFilterBase,
  ListPageMeta,
} from '@/Components/ListPage/type'
import { useLanguage } from '@/core/context/useLanguage'

const DEFAULT_PAGE_SIZE = 20
const DEFAULT_PAGE_SIZE_OPTIONS = ['10', '20', '50', '100']

/**
 * Ortak liste kabuğu:
 * FormProvider filtre, … menü (filtre kaydet + kolon ayarları), sort/page, `onDetailPage`.
 */
export function ConnectedListPage<
  TRecord extends object,
  TFilter extends ListPageFilterBase = ListPageFilterBase,
>({
  storageKey,
  title,
  subtitle,
  brandEyebrow,
  clearFiltersLabel,
  saveFiltersLabel,
  filtersSavedLabel,
  refreshLabel,
  createLabel,
  listCountLabel,
  emptyTitle,
  emptyDescription,
  emptySearchTitle,
  emptySearchDescription,
  searchLabel,
  searchSubmitLabel,
  emptyAction,
  primaryActions,
  filterContent,
  filterExtra,
  filterSchema,
  defaultFilter,
  defaultMeta,
  columns,
  dataSource,
  rowKey,
  totalCount,
  loading = false,
  fetching = false,
  pageSizeOptions = DEFAULT_PAGE_SIZE_OPTIONS,
  enableFilterPersistence = true,
  enableColumnSettings = true,
  enableCreate = false,
  onNeedData,
  onRefetch,
  onDetailPage,
  tableProps,
}: ConnectedListPageProps<TRecord, TFilter>) {
  const { message } = App.useApp()
  const { t } = useLanguage()
  const onNeedDataRef = useRef(onNeedData)

  const resolvedDefaultFilter = (defaultFilter ??
    defaultListFilterValues) as DefaultValues<TFilter>
  const schema = (filterSchema ?? defaultListFilterSchema) as ZodType<TFilter>

  const initial = resolveInitialState(
    storageKey,
    enableFilterPersistence,
    resolvedDefaultFilter as TFilter,
    defaultMeta,
  )

  const columnKeysSignature = columns
    .map((column) => {
      const key =
        typeof column.key === 'string' || typeof column.key === 'number'
          ? String(column.key)
          : 'dataIndex' in column && typeof column.dataIndex === 'string'
            ? column.dataIndex
            : ''
      const width = typeof column.width === 'number' ? String(column.width) : ''
      return `${key}:${width}`
    })
    .join('|')

  const defaultColumnPrefs = useMemo(
    () => buildDefaultColumnPreferences(columns),
    // Parent her render’da yeni columns dizisi verir; kimlik imzasına bağlanır.
    // eslint-disable-next-line react-hooks/exhaustive-deps -- columnKeysSignature
    [columnKeysSignature],
  )

  const [appliedFilter, setAppliedFilter] = useState<TFilter>(initial.filter)
  const [meta, setMeta] = useState<ListPageMeta>(initial.meta)
  const [columnPrefs, setColumnPrefs] = useState<ListColumnPreference[]>(() =>
    mergeColumnPreferences(
      buildDefaultColumnPreferences(columns),
      readColumnPreferences(storageKey),
    ),
  )
  const [columnSettingsOpen, setColumnSettingsOpen] = useState(false)
  const [detail, setDetail] = useState<{
    open: boolean
    reason: ListDetailReason
    record?: TRecord
  }>({ open: false, reason: 'create' })

  const form = useForm<TFilter>({
    // Zod 4 + generic TFilter: zodResolver / RHF Resolver köprüsü.
    resolver: zodResolver(schema as never) as unknown as Resolver<TFilter>,
    defaultValues: initial.filter as DefaultValues<TFilter>,
    mode: 'onSubmit',
  })

  useEffect(() => {
    onNeedDataRef.current = onNeedData
  }, [onNeedData])

  useEffect(() => {
    onNeedDataRef.current(appliedFilter, meta)
  }, [appliedFilter, meta])

  useEffect(() => {
    setColumnPrefs(
      mergeColumnPreferences(defaultColumnPrefs, readColumnPreferences(storageKey)),
    )
  }, [defaultColumnPrefs, storageKey])

  const watchedFilter = form.watch()
  const watchedSearch = String(watchedFilter.search ?? '')
  const canClear =
    hasTrimmed(appliedFilter.search) || Boolean(meta.sorter) || hasTrimmed(watchedSearch)
  const hasActiveFilters = hasTrimmed(appliedFilter.search)
  const isEmpty = !loading && dataSource.length === 0
  const showMoreMenu = enableFilterPersistence || enableColumnSettings

  const displayColumns = useMemo(() => {
    const preferred = enableColumnSettings
      ? applyColumnPreferences(columns, columnPrefs)
      : columns
    return preferred.map((column) => {
      const dataIndex =
        'dataIndex' in column && typeof column.dataIndex === 'string'
          ? column.dataIndex
          : undefined
      const field = dataIndex ?? (typeof column.key === 'string' ? column.key : undefined)
      if (!field || !('sorter' in column) || !column.sorter) return column
      return { ...column, sortOrder: sortOrderForField(meta.sorter, field) }
    })
  }, [columnPrefs, columns, enableColumnSettings, meta.sorter])

  const applyFilterValues = useCallback((values: TFilter) => {
    const next = { ...values, search: String(values.search ?? '').trim() } as TFilter
    setAppliedFilter(next)
    setMeta((prev) => ({ ...prev, page: 1 }))
  }, [])

  const search = useCallback(() => {
    void form.handleSubmit((values) => applyFilterValues(values))()
  }, [applyFilterValues, form])

  const clear = useCallback(() => {
    const cleared = {
      ...(resolvedDefaultFilter as TFilter),
      search: '',
    } as TFilter
    form.reset(cleared)
    setAppliedFilter(cleared)
    setMeta((prev) => ({ ...prev, page: 1, sorter: undefined }))
    if (enableFilterPersistence) clearListPageState(storageKey)
  }, [enableFilterPersistence, form, resolvedDefaultFilter, storageKey])

  const openDetail = useCallback((reason: ListDetailReason, record?: TRecord) => {
    setDetail({ open: true, reason, record })
  }, [])

  const closeDetail = useCallback(() => {
    setDetail((prev) => ({ ...prev, open: false, record: undefined }))
  }, [])

  const handleDetailSaved = useCallback(() => {
    closeDetail()
    onRefetch?.()
  }, [closeDetail, onRefetch])

  const create = useCallback(() => {
    if (onDetailPage) openDetail('create')
  }, [onDetailPage, openDetail])

  const saveFilters = useCallback(() => {
    if (!enableFilterPersistence) return
    writeListPageState(storageKey, { filter: appliedFilter, meta })
    message.success(filtersSavedLabel)
  }, [
    appliedFilter,
    enableFilterPersistence,
    filtersSavedLabel,
    message,
    meta,
    storageKey,
  ])

  const saveColumnPrefs = useCallback(
    (next: ListColumnPreference[]) => {
      writeColumnPreferences(storageKey, { columns: next })
      setColumnPrefs(next)
      setColumnSettingsOpen(false)
      message.success(t('listPage.columnSettingsSaved'))
    },
    [message, storageKey, t],
  )

  const resetColumnPrefs = useCallback(() => {
    clearColumnPreferences(storageKey)
    setColumnPrefs(defaultColumnPrefs.map((item) => ({ ...item })))
  }, [defaultColumnPrefs, storageKey])

  const detailProps: ListDetailPageProps<TRecord> = {
    open: detail.open,
    reason: detail.reason,
    record: detail.record,
    onClose: closeDetail,
    onSaved: handleDetailSaved,
  }

  const commandsValue = useMemo(
    () => ({ search, clear, create, enableCreate: enableCreate && Boolean(onDetailPage) }),
    [search, clear, create, enableCreate, onDetailPage],
  )

  const moreMenuItems = useMemo(() => {
    const items: {
      key: string
      icon: ReactNode
      label: string
      onClick: () => void
    }[] = []
    if (enableFilterPersistence) {
      items.push({
        key: 'saveFilters',
        icon: <SaveOutlined />,
        label: saveFiltersLabel,
        onClick: saveFilters,
      })
    }
    if (enableColumnSettings) {
      items.push({
        key: 'columnSettings',
        icon: <TableOutlined />,
        label: t('listPage.columnSettings'),
        onClick: () => setColumnSettingsOpen(true),
      })
    }
    return items
  }, [
    enableColumnSettings,
    enableFilterPersistence,
    saveFilters,
    saveFiltersLabel,
    t,
  ])

  return (
    <FormProvider {...form}>
      <ListPageCommandsProvider value={commandsValue}>
        <div className="list-page app-shell-rise space-y-8">
          <header className="list-page-header">
            <div className="space-y-2.5">
              {brandEyebrow ? (
                <p className="inline-flex items-center gap-2 text-[11px] font-semibold uppercase tracking-[0.16em] text-brand">
                  <span className="h-1.5 w-1.5 rounded-full bg-brand shadow-[0_0_0_3px_rgb(var(--brand-primary)/0.2)]" aria-hidden />
                  {brandEyebrow}
                </p>
              ) : null}
              <h1 className="text-[1.85rem] font-semibold tracking-tight text-primary sm:text-[2rem]">
                {title}
              </h1>
              {subtitle ? (
                <p className="max-w-2xl text-[14px] leading-relaxed text-secondary">{subtitle}</p>
              ) : null}
            </div>
            <div className="flex flex-wrap items-center gap-2">
              {onRefetch ? (
                <button
                  type="button"
                  className="list-page-btn list-page-btn--ghost"
                  onClick={onRefetch}
                  disabled={fetching}
                >
                  <ReloadOutlined className={fetching ? 'animate-spin' : undefined} aria-hidden />
                  {refreshLabel}
                </button>
              ) : null}
              {enableCreate && onDetailPage ? (
                <button
                  type="button"
                  className="list-page-btn list-page-btn--primary"
                  onClick={create}
                >
                  <PlusOutlined aria-hidden />
                  {createLabel}
                </button>
              ) : null}
              {primaryActions}
            </div>
          </header>

          <section className="list-page-panel">
            <div className="list-page-toolbar">
              <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
                <div className="flex min-w-0 flex-1 flex-col gap-3 sm:flex-row sm:items-end">
                  {filterContent ??
                    (searchLabel && searchSubmitLabel ? (
                      <DefaultListSearchFilter
                        searchLabel={searchLabel}
                        searchSubmitLabel={searchSubmitLabel}
                      />
                    ) : null)}
                  {filterExtra}
                </div>
                <div className="flex flex-wrap items-center gap-2 lg:justify-end">
                  <span className="list-page-count">{listCountLabel(totalCount)}</span>
                  <button
                    type="button"
                    className="list-page-btn list-page-btn--quiet"
                    onClick={clear}
                    disabled={!canClear}
                  >
                    {clearFiltersLabel}
                  </button>
                  {showMoreMenu ? (
                    <Dropdown menu={{ items: moreMenuItems }} trigger={['click']} placement="bottomRight">
                      <button
                        type="button"
                        className="list-page-btn list-page-btn--quiet !h-8 !w-8 !px-0"
                        aria-label={t('listPage.moreActions')}
                      >
                        <MoreOutlined aria-hidden />
                      </button>
                    </Dropdown>
                  ) : null}
                </div>
              </div>
            </div>

            {isEmpty ? (
              <div className="px-5 py-16">
                <Empty
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                  description={
                    <div className="space-y-1.5">
                      <p className="text-sm font-medium text-primary">
                        {hasActiveFilters ? emptySearchTitle : emptyTitle}
                      </p>
                      <p className="text-sm text-secondary">
                        {hasActiveFilters
                          ? emptySearchDescription(String(appliedFilter.search ?? '').trim())
                          : emptyDescription}
                      </p>
                    </div>
                  }
                >
                  {hasActiveFilters ? (
                    <button
                      type="button"
                      className="list-page-btn list-page-btn--ghost"
                      onClick={clear}
                    >
                      {clearFiltersLabel}
                    </button>
                  ) : (
                    (emptyAction ??
                      (enableCreate && onDetailPage ? (
                        <button
                          type="button"
                          className="list-page-btn list-page-btn--primary"
                          onClick={create}
                        >
                          <PlusOutlined aria-hidden />
                          {createLabel}
                        </button>
                      ) : null))
                  )}
                </Empty>
              </div>
            ) : (
              <Table<TRecord>
                {...tableProps}
                className={['list-page-table', tableProps?.className].filter(Boolean).join(' ')}
                size="middle"
                rowKey={rowKey}
                loading={loading}
                dataSource={dataSource}
                columns={displayColumns}
                pagination={{
                  current: meta.page,
                  pageSize: meta.pageSize,
                  total: totalCount,
                  showSizeChanger: true,
                  pageSizeOptions,
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

        {enableColumnSettings ? (
          <ListColumnSettingsDrawer
            open={columnSettingsOpen}
            columns={columns}
            preferences={columnPrefs}
            onClose={() => setColumnSettingsOpen(false)}
            onSave={saveColumnPrefs}
            onReset={resetColumnPrefs}
          />
        ) : null}

        {onDetailPage?.(detailProps)}
      </ListPageCommandsProvider>
    </FormProvider>
  )
}

function hasTrimmed(value: unknown): boolean {
  return String(value ?? '').trim().length > 0
}

function resolveInitialState<TFilter extends ListPageFilterBase>(
  storageKey: string,
  enableFilterPersistence: boolean,
  defaultFilter: TFilter,
  defaultMeta: Partial<ListPageMeta> | undefined,
): { filter: TFilter; meta: ListPageMeta } {
  const fallbackMeta: ListPageMeta = {
    page: defaultMeta?.page ?? 1,
    pageSize: defaultMeta?.pageSize ?? DEFAULT_PAGE_SIZE,
    sorter: defaultMeta?.sorter,
  }

  if (!enableFilterPersistence) {
    return { filter: { ...defaultFilter, search: String(defaultFilter.search ?? '') }, meta: fallbackMeta }
  }

  const stored = readListPageState<TFilter>(storageKey)
  if (!stored) {
    return { filter: { ...defaultFilter, search: String(defaultFilter.search ?? '') }, meta: fallbackMeta }
  }

  return {
    filter: { ...defaultFilter, ...stored.filter, search: String(stored.filter.search ?? '') },
    meta: {
      page: stored.meta.page || 1,
      pageSize: stored.meta.pageSize || DEFAULT_PAGE_SIZE,
      sorter: stored.meta.sorter,
    },
  }
}
