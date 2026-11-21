import type { ListPageFilterBase, ListPagePersistedState } from '@/Components/ListPage/type'

const STORAGE_PREFIX = 'atagreen.listPage.'

function storageKey(key: string): string {
  return `${STORAGE_PREFIX}${key}`
}

/** Liste filtre/meta durumunu localStorage’dan okur. */
export function readListPageState<TFilter extends ListPageFilterBase>(
  key: string,
): ListPagePersistedState<TFilter> | null {
  try {
    const raw = localStorage.getItem(storageKey(key))
    if (!raw) return null
    const parsed = JSON.parse(raw) as ListPagePersistedState<TFilter>
    if (!parsed || typeof parsed !== 'object' || !parsed.filter || !parsed.meta) return null
    return parsed
  } catch {
    return null
  }
}

/** Liste filtre/meta durumunu localStorage’a yazar (kullanıcı “filtreleri kaydet”). */
export function writeListPageState<TFilter extends ListPageFilterBase>(
  key: string,
  state: ListPagePersistedState<TFilter>,
): void {
  try {
    localStorage.setItem(storageKey(key), JSON.stringify(state))
  } catch {
    // Quota / private mode — sessizce yut.
  }
}

/** Kayıtlı liste durumunu siler. */
export function clearListPageState(key: string): void {
  try {
    localStorage.removeItem(storageKey(key))
  } catch {
    // ignore
  }
}
