import i18n, { namespaces } from '../../i18n'
import type { Language } from './languages'

/** Tüm namespace'lerdeki düz çeviri anahtarları (geriye dönük `t('menu.dashboard')` uyumu). */
export type MessageKey = string

function interpolationFromParams(params: readonly unknown[]): Record<string, unknown> {
  return Object.fromEntries(params.map((value, index) => [String(index), value]))
}

type Namespace = (typeof namespaces)[number]

/** Anahtarın hangi namespace'te olduğunu bulur; bulunamazsa defaultNS (`common`) kullanılır. */
function resolveNamespace(key: string, language?: Language): Namespace {
  for (const ns of namespaces) {
    if (i18n.exists(key, { ns, lng: language })) return ns
  }
  return 'common'
}

/**
 * UI metnini react-i18next üzerinden çözer.
 * Eski `{0}` yer tutucuları JSON'da `{{0}}` olarak saklanır; `params` sırayla dolar.
 */
export function translate(language: Language, key: MessageKey, params: readonly unknown[] = []): string {
  const ns = resolveNamespace(key, language)
  const t = i18n.getFixedT(language, ns)
  return t(key as never, interpolationFromParams(params))
}

export function hasMessage(key: string): boolean {
  return namespaces.some((ns) => i18n.exists(key, { ns }))
}
