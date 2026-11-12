import { EditOutlined, PlusOutlined, ReloadOutlined } from '@ant-design/icons'
import { Button, Flex, Segmented, Space, Table, Typography } from 'antd'
import { useState } from 'react'
import { StatusTag } from '../../components/StatusTag'
import { TranslationTags } from '../../components/TranslationTags'
import { useApiQuery } from '../../core/api/useApiQuery'
import { useLanguage } from '../../core/context/useLanguage'
import { DEFAULT_LANGUAGE } from '../../core/i18n/languages'
import { unitOfMeasureService } from '../../services/unitOfMeasureService'
import type { UnitOfMeasure } from '../../types/unitOfMeasure'
import { UnitOfMeasureFormModal } from './UnitOfMeasureFormModal'

type StatusFilter = 'all' | 'active' | 'inactive'

export function UnitsOfMeasurePage() {
  const { language, t } = useLanguage()
  const [status, setStatus] = useState<StatusFilter>('all')
  // Kapanış animasyonu bitene kadar başlık ve form değişmesin diye düzenlenen birim modal kapanınca da tutulur.
  const [editor, setEditor] = useState<{ open: boolean; unit: UnitOfMeasure | null }>({ open: false, unit: null })

  const { data, loading, reload } = useApiQuery(
    (signal) =>
      unitOfMeasureService.list({ language, isActive: status === 'all' ? undefined : status === 'active' }, signal),
    [language, status],
  )

  const openEditor = (unit: UnitOfMeasure | null) => setEditor({ open: true, unit })
  const closeEditor = () => setEditor((current) => ({ ...current, open: false }))

  return (
    <Space orientation="vertical" size="large" style={{ width: '100%' }}>
      <Flex justify="space-between" align="center" wrap gap={16}>
        <Typography.Title level={3} style={{ margin: 0 }}>
          {t('menu.unitsOfMeasure')}
        </Typography.Title>
        <Space wrap>
          <Segmented<StatusFilter>
            value={status}
            onChange={setStatus}
            options={[
              { value: 'all', label: t('common.all') },
              { value: 'active', label: t('list.active') },
              { value: 'inactive', label: t('list.inactive') },
            ]}
          />
          <Button icon={<ReloadOutlined />} onClick={reload}>
            {t('common.refresh')}
          </Button>
          <Button type="primary" icon={<PlusOutlined />} onClick={() => openEditor(null)}>
            {t('uom.create')}
          </Button>
        </Space>
      </Flex>

      <Table<UnitOfMeasure>
        rowKey="id"
        loading={loading}
        dataSource={data}
        pagination={false}
        columns={[
          { title: t('list.code'), dataIndex: 'code', width: 140 },
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
          {
            key: 'actions',
            width: 120,
            align: 'right',
            render: (_, unit) => (
              <Button type="link" icon={<EditOutlined />} onClick={() => openEditor(unit)}>
                {t('common.edit')}
              </Button>
            ),
          },
        ]}
      />

      <UnitOfMeasureFormModal
        open={editor.open}
        unit={editor.unit}
        onClose={closeEditor}
        onSaved={() => {
          closeEditor()
          reload()
        }}
      />
    </Space>
  )
}
