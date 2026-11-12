import { Flex, Select, Tag, Typography, type SelectProps } from 'antd'
import { useLanguage } from '../../core/context/useLanguage'
import type { ParentOption } from './categoryForm'

type ParentCategorySelectProps = Omit<SelectProps<string, ParentOption>, 'options' | 'showSearch' | 'optionRender'> & {
  options: ParentOption[]
}

/** Kategoriler kökten itibaren yol olarak listelenir; ad yolu veya kodla aranabilir. */
export function ParentCategorySelect({ options, ...props }: ParentCategorySelectProps) {
  const { t } = useLanguage()

  return (
    <Select<string, ParentOption>
      allowClear
      showSearch={{ optionFilterProp: ['label', 'code'] }}
      options={options}
      optionRender={({ data }) => (
        <Flex justify="space-between" align="center" gap={8}>
          <Typography.Text ellipsis>{data.label}</Typography.Text>
          <span>
            {!data.isActive && <Tag>{t('list.inactive')}</Tag>}
            {data.code && (
              <Typography.Text type="secondary" code>
                {data.code}
              </Typography.Text>
            )}
          </span>
        </Flex>
      )}
      {...props}
    />
  )
}
