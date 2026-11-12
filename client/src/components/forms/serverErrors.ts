import { toFieldPath } from '../../core/api/apiError'

export type FieldPath = (string | number)[]

export interface ServerFieldError {
  name: FieldPath
  errors: string[]
}

export interface TranslationErrorMapping {
  /** Gönderilen çeviriler; sunucudaki `Translations[i]` dizini bu listedeki sıradır. */
  sent: readonly { languageCode: string }[]
  /** Çeviri başına alanlar (ör. `name`, `description`). */
  fields: readonly string[]
}

/**
 * Sunucunun 400 yanıtındaki alan hatalarını form alanlarına bağlar. Çevirilerde `Translations[i]` dizini gönderilen
 * listedeki sıradır; form ise çevirileri dil koduyla tuttuğundan dizin dile çevrilir. Bir alana bağlanamayan hatalar
 * (ör. liste düzeyindeki kurallar) `unmatched` olarak döner.
 *
 * @param rootFields Formdaki üst düzey alanlar (ör. `code`, `warehouseId`).
 */
export function mapServerFieldErrors(
  fieldErrors: Record<string, string[]>,
  rootFields: readonly string[],
  translations?: TranslationErrorMapping,
): { fields: ServerFieldError[]; unmatched: string[] } {
  const fields: ServerFieldError[] = []
  const unmatched: string[] = []

  for (const [path, errors] of Object.entries(fieldErrors)) {
    const [root, index, property, ...rest] = toFieldPath(path)
    const translation = typeof index === 'number' ? translations?.sent[index] : undefined

    if (typeof root === 'string' && rootFields.includes(root) && index === undefined) {
      fields.push({ name: [root], errors })
    } else if (
      root === 'translations' &&
      translation &&
      typeof property === 'string' &&
      translations?.fields.includes(property) &&
      rest.length === 0
    ) {
      fields.push({ name: ['translations', translation.languageCode, property], errors })
    } else {
      unmatched.push(...errors)
    }
  }

  return { fields, unmatched }
}
