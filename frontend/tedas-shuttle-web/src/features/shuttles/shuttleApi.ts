import { apiClient } from '../../api/httpClient'
import type { PaginatedList } from '../../types/pagination'
import type {
  Shuttle,
  ShuttleFormValues,
  ShuttleListItem,
  ShuttleQuery,
  UpdateShuttlePayload,
} from './shuttleTypes'

export async function searchShuttles(query: ShuttleQuery) {
  const response = await apiClient.get<PaginatedList<ShuttleListItem>>('/api/shuttles', {
    params: query,
  })

  return response.data
}

export async function getShuttle(id: string) {
  const response = await apiClient.get<Shuttle>(`/api/shuttles/${id}`)

  return response.data
}

export async function createShuttle(values: ShuttleFormValues) {
  const response = await apiClient.post<Shuttle>('/api/shuttles', toApiPayload(values))

  return response.data
}

export async function updateShuttle(id: string, values: UpdateShuttlePayload) {
  const response = await apiClient.put<Shuttle>(`/api/shuttles/${id}`, toApiPayload(values))

  return response.data
}

export async function updateShuttleStatus(id: string, isActive: boolean) {
  const response = await apiClient.patch<Shuttle>(`/api/shuttles/${id}/status`, { isActive })

  return response.data
}

function toApiPayload<T extends Record<string, unknown>>(values: T) {
  return Object.fromEntries(
    Object.entries(values).map(([key, value]) => [
      key,
      typeof value === 'string' && value.trim() === '' ? null : value,
    ]),
  )
}
