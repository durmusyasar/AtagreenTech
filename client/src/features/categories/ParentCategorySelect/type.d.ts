import type { SelectProps } from 'antd'
import type { ParentOption } from '../categoryForm'

export type ParentCategorySelectProps = Omit<
  SelectProps<string, ParentOption>,
  'options' | 'showSearch' | 'optionRender'
> & {
  options: ParentOption[]
}
