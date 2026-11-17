import { Tag } from 'antd'
import { useLanguage } from '@/core/context/useLanguage'
import type { MovementType } from '@/types/stock'
import type { MovementTypeTagProps } from '@/routes/catalog/stock/MovementTypeTag/type'

const COLORS: Record<MovementType, string> = { Inbound: 'green', Outbound: 'volcano', Transfer: 'blue' }

export function MovementTypeTag({ type }: MovementTypeTagProps) {
  const { t } = useLanguage()
  return <Tag color={COLORS[type]}>{t(`movement.type.${type}`)}</Tag>
}
