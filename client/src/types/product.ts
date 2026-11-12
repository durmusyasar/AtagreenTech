import type { PageRequest } from './api'

export interface Product {
  id: string
  name: string
  sku: string
  description: string | null
  unitPrice: number
  categoryId: string
  categoryName: string
  unitOfMeasureId: string
  unitOfMeasureCode: string
  isActive: boolean
  createdAt: string
}

export interface ProductQuery extends PageRequest {
  searchTerm?: string
  categoryId?: string
  isActive?: boolean
}
