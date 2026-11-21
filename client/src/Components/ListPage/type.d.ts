import type { ReactNode } from 'react'
import type { TableColumnsType, TablePaginationConfig, TableProps } from 'antd'
import type { FilterValue, SorterResult, TableCurrentDataSource } from 'antd/es/table/interface'

/** Liste sıralaması — Ant Design Table `sorter` ile uyumlu. */
export interface ListPageSorter {
  field: string
  order: 'ascend' | 'descend'
}

/** Sayfalama + sıralama meta bilgisi (`onNeedData` ile API’ye gider). */
export interface ListPageMeta {
  page: number
  pageSize: number
  sorter?: ListPageSorter
}

/** Varsayılan filtre şekli; ekranlar genişletebilir. */
export interface ListPageFilterBase {
  search: string
}

/** Yerel olarak saklanan liste durumu (filtre + sayfa + sıralama). */
export interface ListPagePersistedState<TFilter extends ListPageFilterBase = ListPageFilterBase> {
  filter: TFilter
  meta: ListPageMeta
}

export interface ConnectedListPageProps<
  TRecord extends object,
  TFilter extends ListPageFilterBase = ListPageFilterBase,
> {
  /** localStorage anahtarı (ör. `system/tenants`). */
  storageKey: string
  title: string
  subtitle?: string
  brandEyebrow?: string
  searchLabel: string
  searchSubmitLabel: string
  clearFiltersLabel: string
  saveFiltersLabel: string
  filtersSavedLabel: string
  refreshLabel: string
  listCountLabel: (total: number) => string
  emptyTitle: string
  emptyDescription: string
  emptySearchTitle: string
  emptySearchDescription: (search: string) => string
  /** Boş durum birincil CTA (arama yokken). */
  emptyAction?: ReactNode
  /** Toolbar sağ aksiyonlar (ör. Yeni kayıt). */
  primaryActions?: ReactNode
  /** Arama sağına ek filtre slot’u. */
  filterExtra?: ReactNode
  columns: TableColumnsType<TRecord>
  dataSource: TRecord[]
  rowKey: TableProps<TRecord>['rowKey']
  totalCount: number
  loading?: boolean
  fetching?: boolean
  pageSizeOptions?: string[]
  enableFilterPersistence?: boolean
  /** Varsayılan filtre (persistence yoksa veya kayıt yoksa). */
  defaultFilter?: TFilter
  defaultMeta?: Partial<ListPageMeta>
  /** Filtre/sayfa/sıralama değişince çağrılır — parent API sorgusunu buradan bağlar. */
  onNeedData: (filter: TFilter, meta: ListPageMeta) => void
  onRefetch?: () => void
  /** Tablo ekstra props (scroll vb.). */
  tableProps?: Omit<
    TableProps<TRecord>,
    'columns' | 'dataSource' | 'loading' | 'pagination' | 'onChange' | 'rowKey'
  >
}

export type ListPageTableChangeHandler<TRecord extends object> = (
  pagination: TablePaginationConfig,
  filters: Record<string, FilterValue | null>,
  sorter: SorterResult<TRecord> | SorterResult<TRecord>[],
  extra: TableCurrentDataSource<TRecord>,
) => void
