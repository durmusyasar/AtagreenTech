import { Button, Result } from 'antd'
import { Link } from 'react-router-dom'
import { useLanguage } from '../core/context/useLanguage'

/** Yetkisiz rota erişiminde gösterilen 403 ekranı. */
export function Unauthorized() {
  const { t } = useLanguage()

  return (
    <Result
      status="403"
      title={t('unauthorized.title')}
      subTitle={t('unauthorized.description')}
      extra={
        <Link to="/">
          <Button type="primary">{t('unauthorized.back')}</Button>
        </Link>
      }
    />
  )
}
