import type { FormInstance } from 'antd'
import { useState } from 'react'
import { ApiError } from '../../core/api/apiError'
import { mapServerFieldErrors, type ServerFieldError, type TranslationErrorMapping } from './serverErrors'

/** Hatanın gösterileceği form alanı; `undefined` ise hata formun üstündeki uyarıda gösterilir. */
export type BusinessRuleField = string | ((error: ApiError) => string | undefined)

export interface ServerErrorOptions {
  /** Formdaki üst düzey alanlar; sunucunun 400 hataları bu adlarla eşlenir. */
  rootFields: readonly string[]
  /** 422 iş kuralı anahtarlarından, hatanın gösterileceği form alanına (ör. kod çakışması → `code`). */
  businessRuleFields?: Readonly<Record<string, BusinessRuleField>>
  /** 400 hataları alanlara yazıldıktan sonra çağrılır (ör. hatalı sekmeye geçmek için). */
  onFieldErrors?: (fields: ServerFieldError[]) => void
}

/**
 * Sunucu hatalarını formda gösterir: 400 alan hataları ve alanı belli 422'ler ilgili alanın altına, diğerleri formun
 * üstündeki uyarıya yazılır. İsteği `skipErrorNotification` ile göndermek, aynı hatanın global bildirimde de
 * görünmesini önler.
 */
export function useServerErrors<Values>(
  typedForm: FormInstance<Values>,
  { rootFields, businessRuleFields = {}, onFieldErrors }: ServerErrorOptions,
) {
  const form = typedForm as FormInstance
  const [formError, setFormError] = useState<string>()

  const applyServerError = (error: unknown, translations?: TranslationErrorMapping) => {
    // 401'de oturum kapanır ve kullanıcı oturum açma sayfasına yönlendirilir.
    if (!(error instanceof ApiError) || error.status === 401) return

    if (error.status === 400) {
      const { fields, unmatched } = mapServerFieldErrors(error.fieldErrors, rootFields, translations)
      form.setFields(fields)
      onFieldErrors?.(fields)
      if (unmatched.length > 0) setFormError(unmatched.join(' '))
      else if (fields.length === 0) setFormError(error.message)
      return
    }

    const target = error.resourceKey ? businessRuleFields[error.resourceKey] : undefined
    const field = typeof target === 'function' ? target(error) : target
    if (field) form.setFields([{ name: field, errors: [error.message] }])
    else setFormError(error.message)
  }

  return { formError, setFormError, clearFormError: () => setFormError(undefined), applyServerError }
}
