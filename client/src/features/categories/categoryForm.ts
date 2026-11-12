import {
  collectTranslations,
  toTranslationValues,
  type TranslationValues,
} from '../../components/translations/translationForm'
import type { Category, CategoryTranslation } from '../../types/category'

/** Backend'deki `Category` ve `CategoryTranslation` uzunluk sınırları. */
export const CATEGORY_LIMITS = { code: 50, name: 150, description: 500 } as const

export const CATEGORY_TRANSLATION_FIELDS = ['name', 'description'] as const

export type CategoryTranslationField = (typeof CATEGORY_TRANSLATION_FIELDS)[number]

export interface CategoryFormValues {
  code: string
  isActive: boolean
  /** Boş ise kategori ana kategoridir. */
  parentCategoryId?: string | null
  /** Dil koduna göre çeviriler; ad ve açıklaması boş bırakılan diller kaydedilmez. */
  translations: TranslationValues<CategoryTranslationField>
}

export interface ParentOption {
  value: string
  /** Kökten kategoriye kadar adlar: `Sulama / Damla Sulama`. */
  label: string
  code: string
  isActive: boolean
}

export function toFormValues(category?: Category | null): CategoryFormValues {
  return {
    code: category?.code ?? '',
    isActive: category?.isActive ?? true,
    parentCategoryId: category?.parentCategoryId ?? null,
    translations: toTranslationValues(category?.translations ?? []),
  }
}

/** Açıklaması boş bırakılan çeviriler için `null` gönderilir. */
export function toRequestTranslations(
  values: CategoryFormValues,
  languages: readonly string[],
): (CategoryTranslation & { description: string | null })[] {
  return collectTranslations(values.translations, languages, CATEGORY_TRANSLATION_FIELDS).map((translation) => ({
    ...translation,
    description: translation.description || null,
  }))
}

/** Kategorinin kendisi ve tüm alt kategorileri; bunlardan biri üst kategori seçilirse hiyerarşide döngü oluşur. */
export function selfAndDescendantIds(categories: readonly Category[], id: string): Set<string> {
  const childrenOf = new Map<string, string[]>()
  for (const category of categories) {
    if (!category.parentCategoryId) continue
    childrenOf.set(category.parentCategoryId, [...(childrenOf.get(category.parentCategoryId) ?? []), category.id])
  }

  const result = new Set([id])
  const queue = [id]
  while (queue.length > 0) {
    for (const child of childrenOf.get(queue.shift()!) ?? []) {
      if (result.has(child)) continue
      result.add(child)
      queue.push(child)
    }
  }
  return result
}

/**
 * Üst kategori seçimindeki seçenekler; hiyerarşi okunabilsin diye her seçenek kökten itibaren yol olarak gösterilir.
 * Düzenlenen kategori verilirse kendisi ve alt kategorileri listelenmez.
 */
export function parentOptions(categories: readonly Category[], editingId?: string): ParentOption[] {
  const byId = new Map(categories.map((category) => [category.id, category]))
  const excluded = editingId ? selfAndDescendantIds(categories, editingId) : new Set<string>()

  const path = (category: Category): string => {
    const names = [category.name]
    const visited = new Set([category.id])
    let parent = category.parentCategoryId ? byId.get(category.parentCategoryId) : undefined
    // Veride döngü olsa bile sonsuz döngüye girilmez.
    while (parent && !visited.has(parent.id)) {
      names.unshift(parent.name)
      visited.add(parent.id)
      parent = parent.parentCategoryId ? byId.get(parent.parentCategoryId) : undefined
    }
    return names.join(' / ')
  }

  return categories
    .filter((category) => !excluded.has(category.id))
    .map((category) => ({ value: category.id, label: path(category), code: category.code, isActive: category.isActive }))
    .sort((a, b) => a.label.localeCompare(b.label))
}
