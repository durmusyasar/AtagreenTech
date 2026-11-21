import type { ColumnType, ColumnsType } from 'antd/es/table'
import type {
  ListColumnPreference,
  ListColumnPreferencesState,
  ListTableColumn,
} from '@/Components/ListPage/type'

const MIN_WIDTH = 80
const MAX_WIDTH = 800

/** Kolon kimliği — `key` veya string `dataIndex`. */
export function resolveColumnKey<TRecord extends object>(
  column: ColumnType<TRecord>,
): string | null {
  if (typeof column.key === 'string' && column.key.length > 0) return column.key
  if (typeof column.key === 'number') return String(column.key)
  if ('dataIndex' in column) {
    const dataIndex = column.dataIndex
    if (typeof dataIndex === 'string' && dataIndex.length > 0) return dataIndex
    if (Array.isArray(dataIndex) && dataIndex.length > 0) return dataIndex.map(String).join('.')
  }
  return null
}

/** Ayarlar paneli etiketi — string title yoksa key. */
export function resolveColumnLabel<TRecord extends object>(
  column: ColumnType<TRecord>,
  key: string,
): string {
  if (typeof column.title === 'string' && column.title.trim()) return column.title
  return key
}

/** Kolon gizlenemez mi (`listColumnLocked` veya sabit sağ aksiyon). */
export function isColumnLocked<TRecord extends object>(
  column: ColumnType<TRecord> | ListTableColumn<TRecord>,
): boolean {
  const listColumn = column as ListTableColumn<TRecord>
  if (listColumn.listColumnLocked === true) return true
  return column.fixed === 'right'
}

export function clampColumnWidth(width: number | undefined): number | undefined {
  if (width === undefined || Number.isNaN(width)) return undefined
  return Math.min(MAX_WIDTH, Math.max(MIN_WIDTH, Math.round(width)))
}

/** Props sütunlarından varsayılan tercih listesi. */
export function buildDefaultColumnPreferences<TRecord extends object>(
  columns: ColumnsType<TRecord>,
): ListColumnPreference[] {
  const prefs: ListColumnPreference[] = []
  for (const column of columns) {
    if ('children' in column && column.children) continue
    const key = resolveColumnKey(column)
    if (!key) continue
    prefs.push({
      key,
      visible: true,
      width: typeof column.width === 'number' ? clampColumnWidth(column.width) : undefined,
    })
  }
  return prefs
}

/**
 * Saklanan tercihler + güncel kolon setini birleştirir.
 * Bilinmeyen key düşer; yeni kolonlar sona eklenir.
 */
export function mergeColumnPreferences(
  defaults: ListColumnPreference[],
  stored: ListColumnPreferencesState | null,
): ListColumnPreference[] {
  if (!stored?.columns?.length) return defaults.map((item) => ({ ...item }))

  const defaultByKey = new Map(defaults.map((item) => [item.key, item]))
  const merged: ListColumnPreference[] = []
  const seen = new Set<string>()

  for (const item of stored.columns) {
    const fallback = defaultByKey.get(item.key)
    if (!fallback) continue
    seen.add(item.key)
    merged.push({
      key: item.key,
      visible: item.visible,
      width: clampColumnWidth(item.width ?? fallback.width),
    })
  }

  for (const item of defaults) {
    if (seen.has(item.key)) continue
    merged.push({ ...item })
  }

  return merged
}

/** Tercihleri Ant Design columns’a uygular (sıra + görünürlük + genişlik). */
export function applyColumnPreferences<TRecord extends object>(
  columns: ColumnsType<TRecord>,
  preferences: ListColumnPreference[],
): ColumnsType<TRecord> {
  const keyed = new Map<string, ColumnType<TRecord>>()
  const unkeyed: ColumnType<TRecord>[] = []

  for (const column of columns) {
    if ('children' in column && column.children) {
      unkeyed.push(column)
      continue
    }
    const key = resolveColumnKey(column)
    if (!key) {
      unkeyed.push(column)
      continue
    }
    keyed.set(key, column)
  }

  const ordered: ColumnType<TRecord>[] = []
  for (const pref of preferences) {
    const column = keyed.get(pref.key)
    if (!column) continue
    keyed.delete(pref.key)
    if (!pref.visible) continue
    const width = clampColumnWidth(pref.width)
    ordered.push(width !== undefined ? { ...column, width } : { ...column })
  }

  // Tercihte olmayan yeni kolonlar (şimema genişlemesi) sonda görünür kalır.
  for (const leftover of keyed.values()) {
    ordered.push(leftover)
  }

  return [...ordered, ...unkeyed]
}

export const COLUMN_WIDTH_BOUNDS = { min: MIN_WIDTH, max: MAX_WIDTH } as const
