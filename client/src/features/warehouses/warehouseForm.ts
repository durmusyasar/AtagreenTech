import type { CreateWarehouseRequest, Warehouse } from '../../types/warehouse'

/** Backend'deki `Warehouse` uzunluk sınırları. */
export const WAREHOUSE_LIMITS = { code: 50, name: 150, address: 500 } as const

/** Backend'deki `WarehouseValidationRules.CodePattern`: harf, rakam, nokta, tire ve alt çizgi. */
export const WAREHOUSE_CODE_PATTERN = /^[A-Za-z0-9._-]+$/

export interface WarehouseFormValues {
  code: string
  name: string
  address?: string | null
  isActive: boolean
}

export function toFormValues(warehouse?: Warehouse | null): WarehouseFormValues {
  return {
    code: warehouse?.code ?? '',
    name: warehouse?.name ?? '',
    address: warehouse?.address ?? '',
    isActive: warehouse?.isActive ?? true,
  }
}

/** Boş adres `null` olarak gönderilir. */
export function toRequest(values: WarehouseFormValues): CreateWarehouseRequest {
  return { code: values.code.trim(), name: values.name.trim(), address: values.address?.trim() || null }
}
