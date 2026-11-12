import { Tag } from 'antd'
import { useLanguage } from '../../core/context/useLanguage'
import type { MovementType } from '../../types/stock'

const COLORS: Record<MovementType, string> = { Inbound: 'green', Outbound: 'volcano', Transfer: 'blue' }

export function MovementTypeTag({ type }: { type: MovementType }) {
  const { t } = useLanguage()
  return <Tag color={COLORS[type]}>{t(`movement.type.${type}`)}</Tag>
}
