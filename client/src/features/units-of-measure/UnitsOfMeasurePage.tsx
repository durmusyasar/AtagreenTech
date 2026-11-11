import { Alert, Space, Table, Typography } from 'antd'
import { StatusTag } from '../../components/StatusTag'
import { TranslationTags } from '../../components/TranslationTags'
import { useApiQuery } from '../../core/api/useApiQuery'
import { useLanguage } from '../../core/context/useLanguage'
import { DEFAULT_LANGUAGE } from '../../core/i18n/languages'
import { unitOfMeasureService } from '../../services/unitOfMeasureService'
import type { UnitOfMeasure } from '../../types/unitOfMeasure'

export function UnitsOfMeasurePage() {
  const { language, t } = useLanguage()
  const { data, loading } = useApiQuery((signal) => unitOfMeasureService.list({ language }, signal), [language])

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Typography.Title level={3} style={{ margin: 0 }}>
        {t('menu.unitsOfMeasure')}
      </Typography.Title>

      <Alert type="info" showIcon title={t('list.readOnlyNotice', 'Issue #10')} />

      <Table<UnitOfMeasure>
        rowKey="id"
        loading={loading}
        dataSource={data}
        pagination={false}
        columns={[
          { title: t('list.code'), dataIndex: 'code', width: 120 },
          { title: t('list.name'), dataIndex: 'name' },
          { title: t('list.symbol'), dataIndex: 'symbol', width: 120 },
          {
            title: t('list.languages'),
            key: 'languages',
            render: (_, unit) => (
              <TranslationTags
                languages={unit.translations.map((translation) => translation.languageCode)}
                defaultLanguage={DEFAULT_LANGUAGE}
              />
            ),
          },
          {
            title: t('list.status'),
            dataIndex: 'isActive',
            width: 120,
            render: (isActive: boolean) => <StatusTag isActive={isActive} />,
          },
        ]}
      />
    </Space>
  )
}
