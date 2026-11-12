import { isAxiosError } from 'axios'
import type { Language } from '../i18n/languages'
import { hasMessage, translate, type MessageKey } from '../i18n/messages'
import type { ProblemDetails } from '../../types/api'

/**
 * Bir API çağrısının başarısızlığı. `status` ağ hatalarında `null`'dır. Formlar `fieldErrors`'ı (ör.
 * `Translations[1].Name`) alanlara bağlayabilir; `message` kullanıcıya gösterilecek metindir.
 */
export class ApiError extends Error {
  readonly status: number | null
  readonly code: string
  readonly resourceKey: string | null
  readonly parameters: string[]
  readonly fieldErrors: Record<string, string[]>
  readonly traceId: string | null

  constructor(init: {
    message: string
    status: number | null
    code: string
    resourceKey?: string | null
    parameters?: string[]
    fieldErrors?: Record<string, string[]>
    traceId?: string | null
  }) {
    super(init.message)
    this.name = 'ApiError'
    this.status = init.status
    this.code = init.code
    this.resourceKey = init.resourceKey ?? null
    this.parameters = init.parameters ?? []
    this.fieldErrors = init.fieldErrors ?? {}
    this.traceId = init.traceId ?? null
  }
}

export function toApiError(error: unknown, language: Language): ApiError {
  if (error instanceof ApiError) return error

  if (!isAxiosError(error) || !error.response) {
    return new ApiError({ message: translate(language, 'error.network'), status: null, code: 'network_error' })
  }

  const status = error.response.status
  const problem = asProblemDetails(error.response.data)

  return new ApiError({
    message: describe(status, problem, language),
    status,
    code: problem.errorCode ?? `http_${status}`,
    resourceKey: problem.resourceKey,
    parameters: problem.parameters,
    fieldErrors: problem.errors,
    traceId: problem.traceId,
  })
}

/** Sunucudaki alan yolunu form alan yoluna çevirir: `Translations[1].Name` → `['translations', 1, 'name']`. */
export function toFieldPath(path: string): (string | number)[] {
  return (path.match(/[^.[\]]+/g) ?? []).map((part) =>
    /^\d+$/.test(part) ? Number(part) : part.charAt(0).toLowerCase() + part.slice(1),
  )
}

function describe(status: number, problem: ProblemDetails, language: Language): string {
  switch (status) {
    case 400: {
      const messages = Object.values(problem.errors ?? {}).flat()
      return messages.length > 0 ? messages.join(' ') : translate(language, 'error.validation')
    }
    case 401:
      return translate(language, 'error.unauthorized')
    case 403:
      return translate(language, 'error.forbidden')
    case 404:
      return translate(language, 'error.notFound')
    case 422: {
      const key = `error.${problem.resourceKey ?? ''}`
      return hasMessage(key)
        ? translate(language, key as MessageKey, formatParameters(problem, language))
        : (problem.detail ?? translate(language, 'error.businessRule'))
    }
    case 429:
      return translate(language, 'error.rateLimited')
    default:
      return translate(language, 'error.server')
  }
}

/**
 * Sunucu parametreleri kültürden bağımsız yazar (`10.0000`). Hangi parametrenin sayı olduğu anahtara göre bilinir;
 * kodlar da rakam ve nokta içerebildiği için biçimlendirme yalnızca bu sıralara uygulanır.
 */
const NUMERIC_PARAMETERS: Readonly<Record<string, readonly number[]>> = {
  'Inventory.InsufficientStock': [2, 3],
}

function formatParameters(problem: ProblemDetails, language: Language): string[] {
  const parameters = problem.parameters ?? []
  const numeric = NUMERIC_PARAMETERS[problem.resourceKey ?? ''] ?? []
  const format = new Intl.NumberFormat(language, { maximumFractionDigits: 4 })

  return parameters.map((parameter, index) => {
    const value = Number(parameter)
    return numeric.includes(index) && parameter.trim() !== '' && Number.isFinite(value) ? format.format(value) : parameter
  })
}

function asProblemDetails(data: unknown): ProblemDetails {
  return typeof data === 'object' && data !== null ? (data as ProblemDetails) : {}
}
