import { describe, expect, it } from 'vitest'
import { carryOverWarehouse, findWarehouseField, sourceWarehouseField, toMovementRequest } from './movementRequest'

const values = {
  productId: 'p-1',
  warehouseId: 'w-1',
  sourceWarehouseId: 'w-2',
  targetWarehouseId: 'w-3',
  quantity: 12.5,
  unitPrice: 4.25,
  documentReference: '  ',
  description: ' Sevkiyat ',
}

describe('toMovementRequest', () => {
  it('uses the single warehouse for inbound and outbound, trimming texts and sending blanks as null', () => {
    expect(toMovementRequest('Outbound', values)).toEqual({
      type: 'Outbound',
      request: {
        productId: 'p-1',
        warehouseId: 'w-1',
        quantity: 12.5,
        unitPrice: 4.25,
        documentReference: null,
        description: 'Sevkiyat',
      },
    })
  })

  it('uses source and target warehouses for transfers', () => {
    const { request } = toMovementRequest('Transfer', values)
    expect(request).toMatchObject({ sourceWarehouseId: 'w-2', targetWarehouseId: 'w-3' })
    expect(request).not.toHaveProperty('warehouseId')
  })

  it('sends a missing unit price as 0', () => {
    expect(toMovementRequest('Inbound', { ...values, unitPrice: null }).request.unitPrice).toBe(0)
  })
})

describe('sourceWarehouseField', () => {
  it('names the warehouse whose stock decreases', () => {
    expect(sourceWarehouseField('Inbound')).toBeUndefined()
    expect(sourceWarehouseField('Outbound')).toBe('warehouseId')
    expect(sourceWarehouseField('Transfer')).toBe('sourceWarehouseId')
  })
})

describe('carryOverWarehouse', () => {
  it('moves the selected warehouse into the empty warehouse field of the next type', () => {
    expect(carryOverWarehouse('Transfer', { warehouseId: 'w-1' })).toEqual({ sourceWarehouseId: 'w-1' })
    expect(carryOverWarehouse('Outbound', { sourceWarehouseId: 'w-2' })).toEqual({ warehouseId: 'w-2' })
  })

  it('keeps a warehouse the user already chose for the next type', () => {
    expect(carryOverWarehouse('Transfer', { warehouseId: 'w-1', sourceWarehouseId: 'w-2' })).toEqual({})
    expect(carryOverWarehouse('Inbound', { warehouseId: 'w-1', sourceWarehouseId: 'w-2' })).toEqual({})
  })

  it('clears a transfer target that would equal the carried source', () => {
    expect(carryOverWarehouse('Transfer', { warehouseId: 'w-1', targetWarehouseId: 'w-1' })).toEqual({
      sourceWarehouseId: 'w-1',
      targetWarehouseId: undefined,
    })
  })
})

describe('findWarehouseField', () => {
  it('finds the field holding the warehouse reported by the server, ignoring case', () => {
    expect(findWarehouseField('Transfer', values, 'W-3')).toBe('targetWarehouseId')
    expect(findWarehouseField('Inbound', values, 'w-1')).toBe('warehouseId')
  })

  it('ignores fields that are not part of the current movement type', () => {
    expect(findWarehouseField('Inbound', values, 'w-2')).toBeUndefined()
    expect(findWarehouseField('Transfer', values, undefined)).toBeUndefined()
  })
})
