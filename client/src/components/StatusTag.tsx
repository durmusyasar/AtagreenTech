import { Tag } from 'antd'
import { useLanguage } from '../core/context/useLanguage'

export function StatusTag({ isActive }: { isActive: boolean }) {
  const { t } = useLanguage()
  return <Tag color={isActive ? 'success' : 'default'}>{t(isActive ? 'list.active' : 'list.inactive')}</Tag>
}
