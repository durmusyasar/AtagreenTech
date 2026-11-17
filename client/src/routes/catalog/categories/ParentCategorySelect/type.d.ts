import type { SelectProps } from 'antd'
import type { ParentOption } from '@/routes/catalog/categories/categoryForm'

export type ParentCategorySelectProps = Omit<
  SelectProps<string, ParentOption>,
  'options' | 'showSearch' | 'optionRender'
> & {
  options: ParentOption[]
}
