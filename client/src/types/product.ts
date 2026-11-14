import type { PageRequest } from './api'

/** Backend'deki `CurrencyCode.Supported`: ISO 4217 kodları. */
export const CURRENCIES = ['TRY', 'USD', 'EUR'] as const

export type Currency = (typeof CURRENCIES)[number]

export interface ProductTranslation {
  languageCode: string
  name: string
  description?: string | null
}

/** `name`, `description` ve `categoryName` istenen dile göre çözümlenmiştir. */
export interface Product {
  id: string
  sku: string
  /** GS1 barkodu (EAN-8, UPC-A, EAN-13, GTIN-14). */
  barcode: string | null
  name: string
  description: string | null
  /** `currency` cinsinden birim maliyet. */
  unitPrice: number
  currency: string
  categoryId: string
  categoryName: string
  unitOfMeasureId: string
  unitOfMeasureCode: string
  isActive: boolean
  createdAt: string
  translations: ProductTranslation[]
}

export interface ProductQuery extends PageRequest {
  /** SKU, barkod veya herhangi bir dildeki ad. */
  searchTerm?: string
  categoryId?: string
  isActive?: boolean
}

export interface CreateProductRequest {
  sku: string
  barcode: string | null
  unitPrice: number
  currency: string
  categoryId: string
  unitOfMeasureId: string
  translations: ProductTranslation[]
}

export interface UpdateProductRequest extends CreateProductRequest {
  isActive: boolean
}
