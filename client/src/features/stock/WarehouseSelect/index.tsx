import { Flex, Select, Tag, Typography } from 'antd'
import { useMemo } from 'react'
import { useLanguage } from '../../../core/context/useLanguage'
import type { WarehouseOption, WarehouseSelectProps } from './type'

/** Depolar `KOD · Ad` olarak listelenir; kod veya adla aranabilir. */
export function WarehouseSelect({ warehouses, ...props }: WarehouseSelectProps) {
  const { t } = useLanguage()
  const options = useMemo(
    () => warehouses.map((warehouse) => ({ value: warehouse.id, label: `${warehouse.code} · ${warehouse.name}`, warehouse })),
    [warehouses],
  )

  return (
    <Select<string, WarehouseOption>
      allowClear
      showSearch={{ optionFilterProp: 'label' }}
      options={options}
      optionRender={({ data }) => (
        <Flex justify="space-between" align="center" gap={8}>
          <Typography.Text ellipsis>{data.label}</Typography.Text>
          {!data.warehouse.isActive && <Tag>{t('list.inactive')}</Tag>}
        </Flex>
      )}
      {...props}
    />
  )
}
