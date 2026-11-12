import type { PageRequest } from './api'

export type MovementType = 'Inbound' | 'Outbound' | 'Transfer'

interface MovementDetails {
  productId: string
  quantity: number
  /** Hareket anındaki birim fiyat / maliyet. */
  unitPrice: number
  /** Fatura, irsaliye, sevk veya üretim emri numarası. */
  documentReference: string | null
  description: string | null
}

export interface InboundMovementRequest extends MovementDetails {
  warehouseId: string
}

export type OutboundMovementRequest = InboundMovementRequest

export interface TransferMovementRequest extends MovementDetails {
  sourceWarehouseId: string
  targetWarehouseId: string
}

/** Girişlerde kaynak, çıkışlarda hedef depo boştur. */
export interface StockMovement {
  id: string
  type: MovementType
  productId: string
  productSku: string
  productName: string
  sourceWarehouseId: string | null
  sourceWarehouseCode: string | null
  targetWarehouseId: string | null
  targetWarehouseCode: string | null
  quantity: number
  unitPrice: number
  documentReference: string | null
  description: string | null
  createdBy: string | null
  createdAt: string
  clientIpAddress: string | null
  correlationId: string | null
}

export interface StockMovementQuery extends PageRequest {
  productId?: string
  /** Kaynağı veya hedefi bu depo olan hareketler. */
  warehouseId?: string
  type?: MovementType
  from?: string
  to?: string
}

export interface StockBalance {
  warehouseId: string
  warehouseCode: string
  warehouseName: string
  productId: string
  productSku: string
  productName: string
  unitOfMeasureCode: string
  quantity: number
  /** Bakiyeyi değiştiren son hareketin zamanı. */
  lastChangedAt: string
}

export interface StockBalanceQuery extends PageRequest {
  warehouseId?: string
  productId?: string
  /** Miktarı sıfır olan bakiyeleri de listeler. */
  includeEmpty?: boolean
}
