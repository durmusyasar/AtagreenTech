import type { ListColumnPreferencesState } from '@/Components/ListPage/type'

const STORAGE_PREFIX = 'atagreen.listPage.columns.'

function storageKey(key: string): string {
  return `${STORAGE_PREFIX}${key}`
}

/** Kolon görünürlük / sıra / genişlik tercihlerini okur. */
export function readColumnPreferences(key: string): ListColumnPreferencesState | null {
  try {
    const raw = localStorage.getItem(storageKey(key))
    if (!raw) return null
    const parsed = JSON.parse(raw) as ListColumnPreferencesState
    if (!parsed || !Array.isArray(parsed.columns)) return null
    return parsed
  } catch {
    return null
  }
}

/** Kolon tercihlerini yazar. */
export function writeColumnPreferences(key: string, state: ListColumnPreferencesState): void {
  try {
    localStorage.setItem(storageKey(key), JSON.stringify(state))
  } catch {
    // Quota / private mode
  }
}

/** Kolon tercihlerini siler. */
export function clearColumnPreferences(key: string): void {
  try {
    localStorage.removeItem(storageKey(key))
  } catch {
    // ignore
  }
}
