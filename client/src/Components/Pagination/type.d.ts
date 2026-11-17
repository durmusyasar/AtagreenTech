export interface PaginationProps {
  page: number
  pageSize: number
  totalCount: number
  onPageChange: (newPage: number) => void
}
