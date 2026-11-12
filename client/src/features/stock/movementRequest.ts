import type { InboundMovementRequest, MovementType, TransferMovementRequest } from '../../types/stock'

/** Backend'deki `StockMovement` uzunluk sınırları. */
export const MOVEMENT_LIMITS = { documentReference: 100, description: 500 } as const

export const MOVEMENT_TYPES: readonly MovementType[] = ['Inbound', 'Outbound', 'Transfer']

/** Giriş ve çıkışta tek depo (`warehouseId`), transferde kaynak ve hedef depo seçilir. */
export const WAREHOUSE_FIELDS = ['warehouseId', 'sourceWarehouseId', 'targetWarehouseId'] as const

export type WarehouseField = (typeof WAREHOUSE_FIELDS)[number]

export const MOVEMENT_FIELDS = [
  ...WAREHOUSE_FIELDS,
  'productId',
  'quantity',
  'unitPrice',
  'documentReference',
  'description',
] as const

export interface MovementFormValues {
  productId?: string
  warehouseId?: string
  sourceWarehouseId?: string
  targetWarehouseId?: string
  quantity?: number | null
  unitPrice?: number | null
  documentReference?: string
  description?: string
}

export type MovementRequest =
  | { type: 'Inbound' | 'Outbound'; request: InboundMovementRequest }
  | { type: 'Transfer'; request: TransferMovementRequest }

/** Stoğu düşen depo alanı; giriş stoğu düşürmediği için yoktur. Mevcut stok bu depo için gösterilir. */
export function sourceWarehouseField(type: MovementType): WarehouseField | undefined {
  if (type === 'Outbound') return 'warehouseId'
  if (type === 'Transfer') return 'sourceWarehouseId'
  return undefined
}

/**
 * Tür değişince seçili depo, yeni türün boş depo alanına taşınır: çıkışta seçilen depo transferde kaynak olur,
 * transferin kaynağı giriş/çıkışta depo olur. Kullanıcının o alanda daha önce seçtiği depo korunur.
 */
export function carryOverWarehouse(nextType: MovementType, values: MovementFormValues): MovementFormValues {
  if (nextType === 'Transfer') {
    const source = values.warehouseId
    if (values.sourceWarehouseId || !source) return {}
    return values.targetWarehouseId === source
      ? { sourceWarehouseId: source, targetWarehouseId: undefined }
      : { sourceWarehouseId: source }
  }
  return !values.warehouseId && values.sourceWarehouseId ? { warehouseId: values.sourceWarehouseId } : {}
}

/** Form doğrulandıktan sonra çağrılır; zorunlu alanların dolu olduğu varsayılır. Boş metinler `null` gönderilir. */
export function toMovementRequest(type: MovementType, values: MovementFormValues): MovementRequest {
  const details = {
    productId: values.productId!,
    quantity: values.quantity!,
    unitPrice: values.unitPrice ?? 0,
    documentReference: values.documentReference?.trim() || null,
    description: values.description?.trim() || null,
  }

  return type === 'Transfer'
    ? {
        type,
        request: {
          ...details,
          sourceWarehouseId: values.sourceWarehouseId!,
          targetWarehouseId: values.targetWarehouseId!,
        },
      }
    : { type, request: { ...details, warehouseId: values.warehouseId! } }
}

/**
 * Depo bulunamadı veya pasif hatası depo kimliğini parametre olarak taşır; hata, o depoyu seçen alanın altında
 * gösterilir. Transferde kaynak ve hedef ayrı alanlar olduğundan alan, değere göre bulunur.
 */
export function findWarehouseField(
  type: MovementType,
  values: MovementFormValues,
  warehouseId: string | undefined,
): WarehouseField | undefined {
  if (!warehouseId) return undefined
  const fields: readonly WarehouseField[] =
    type === 'Transfer' ? ['sourceWarehouseId', 'targetWarehouseId'] : ['warehouseId']
  return fields.find((field) => values[field]?.toLowerCase() === warehouseId.toLowerCase())
}
