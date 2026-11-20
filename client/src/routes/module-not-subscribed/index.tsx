import { LockOutlined } from '@ant-design/icons'
import { Button, Result } from 'antd'
import { Link } from 'react-router-dom'
import { useLanguage } from '@/core/context/useLanguage'
import type { ModuleNotSubscribedPageProps } from '@/routes/module-not-subscribed/type'

/** Abone olunmayan modül deep-link / guard ekranı (#67). */
export function ModuleNotSubscribedPage({ moduleCode }: ModuleNotSubscribedPageProps) {
  const { t } = useLanguage()
  const moduleLabel = t(`modules.${moduleCode}` as never) || moduleCode

  return (
    <Result
      icon={<LockOutlined />}
      title={t('moduleUnavailable.title')}
      subTitle={t('moduleUnavailable.description', moduleLabel)}
      extra={
        <Link to="/">
          <Button type="primary">{t('moduleUnavailable.back')}</Button>
        </Link>
      }
    />
  )
}
