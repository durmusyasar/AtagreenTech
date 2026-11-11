import { Button, Result } from 'antd'
import { Link } from 'react-router-dom'
import { useLanguage } from '../core/context/useLanguage'

export function NotFound() {
  const { t } = useLanguage()

  return (
    <Result
      status="404"
      title="404"
      subTitle={t('notFound.title')}
      extra={
        <Link to="/">
          <Button type="primary">{t('notFound.back')}</Button>
        </Link>
      }
    />
  )
}
