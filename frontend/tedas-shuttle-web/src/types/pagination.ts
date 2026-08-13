export type PaginatedList<T> = {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}
