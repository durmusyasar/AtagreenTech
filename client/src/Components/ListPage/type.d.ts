import type { ReactNode } from 'react'
import type { ColumnType, TableColumnsType, TablePaginationConfig, TableProps } from 'antd'
import type { FilterValue, SorterResult, TableCurrentDataSource } from 'antd/es/table/interface'
import type { DefaultValues, FieldValues } from 'react-hook-form'
import type { ZodType } from 'zod'

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
export interface ListPageFilterBase extends FieldValues {
  search: string
}

/** Yerel olarak saklanan liste durumu (filtre + sayfa + sıralama). */
export interface ListPagePersistedState<TFilter extends ListPageFilterBase = ListPageFilterBase> {
  filter: TFilter
  meta: ListPageMeta
}

/** Tek kolon için kullanıcı tercihi (görünürlük / sıra / genişlik). */
export interface ListColumnPreference {
  key: string
  visible: boolean
  width?: number
}

/** `localStorage` kolon tercihleri (`atagreen.listPage.columns.<storageKey>`). */
export interface ListColumnPreferencesState {
  columns: ListColumnPreference[]
}

/**
 * Liste kolon sözleşmesi — `key`/`dataIndex` tercih kimliği;
 * `listColumnLocked` gizlenemez (ör. aksiyon sütunu).
 */
export type ListTableColumn<TRecord extends object> = ColumnType<TRecord> & {
  listColumnLocked?: boolean
}

export type ListTableColumnsType<TRecord extends object> = ListTableColumn<TRecord>[]

/** Detail modal/drawer reason — Arkas NeedDataReason sadeleştirmesi. */
export type ListDetailReason = 'create' | 'edit' | 'view'

/** `onDetailPage` render props. */
export interface ListDetailPageProps<TRecord extends object> {
  open: boolean
  reason: ListDetailReason
  record?: TRecord
  onClose: () => void
  /** Kayıt başarılı — liste refetch + genelde kapat. */
  onSaved: () => void
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
  clearFiltersLabel: string
  saveFiltersLabel: string
  filtersSavedLabel: string
  refreshLabel: string
  createLabel?: string
  listCountLabel: (total: number) => string
  emptyTitle: string
  emptyDescription: string
  emptySearchTitle: string
  emptySearchDescription: (search: string) => string
  /** Varsayılan arama etiketi — `filterContent` yoksa built-in arama için. */
  searchLabel?: string
  searchSubmitLabel?: string
  /** Boş durum birincil CTA (arama yokken); yoksa create tetiklenir. */
  emptyAction?: ReactNode
  /** Toolbar sağ ekstra aksiyonlar (create butonu `enableCreate` ile gelir). */
  primaryActions?: ReactNode
  /**
   * Filtre alanları (FormProvider çocuğu). Verilmezse yalnızca `search` FloatingInput.
   * Örn. `<TenantsFilter />`.
   */
  filterContent?: ReactNode
  /** Arama sağına ek slot (nadiren). */
  filterExtra?: ReactNode
  /** Zod filtre şeması — FormProvider resolver. */
  filterSchema?: ZodType<TFilter>
  /** Filtre varsayılanları. */
  defaultFilter?: DefaultValues<TFilter>
  defaultMeta?: Partial<ListPageMeta>
  columns: ListTableColumnsType<TRecord> | TableColumnsType<TRecord>
  dataSource: TRecord[]
  rowKey: TableProps<TRecord>['rowKey']
  totalCount: number
  loading?: boolean
  fetching?: boolean
  pageSizeOptions?: string[]
  enableFilterPersistence?: boolean
  /** Kolon görünürlük / sıra / genişlik ayarları (varsayılan açık). */
  enableColumnSettings?: boolean
  /** Toolbar’da Yeni / empty CTA create. */
  enableCreate?: boolean
  /** Filtre/sayfa/sıralama değişince — parent API sorgusu. */
  onNeedData: (filter: TFilter, meta: ListPageMeta) => void
  onRefetch?: () => void
  /**
   * Gömülü detail (create/edit/view). Arkas `onDetailPage` benzeri.
   * Verilmezse create parent `primaryActions` / dış modal ile yönetilir.
   */
  onDetailPage?: (props: ListDetailPageProps<TRecord>) => ReactNode
  tableProps?: Omit<
    TableProps<TRecord>,
    'columns' | 'dataSource' | 'loading' | 'pagination' | 'onChange' | 'rowKey'
  >
}

/** Filtre toolbar komutları — `filterContent` Enter/ara ikonu için. */
export interface ListPageCommandsContextValue {
  search: () => void
  clear: () => void
  create: () => void
  enableCreate: boolean
}

export type ListPageTableChangeHandler<TRecord extends object> = (
  pagination: TablePaginationConfig,
  filters: Record<string, FilterValue | null>,
  sorter: SorterResult<TRecord> | SorterResult<TRecord>[],
  extra: TableCurrentDataSource<TRecord>,
) => void
