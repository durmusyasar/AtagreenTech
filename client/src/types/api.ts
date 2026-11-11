/**
 * Stok servisinin hata yanıtı (RFC 9457 problem details). 400'de `errors`, 422'de `resourceKey` ve `parameters` dolar.
 */
export interface ProblemDetails {
  type?: string
  title?: string
  status?: number
  detail?: string
  instance?: string
  errorCode?: string
  traceId?: string
  errors?: Record<string, string[]>
  resourceKey?: string
  parameters?: string[]
}

export interface PagedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface PageRequest {
  pageNumber?: number
  pageSize?: number
}
