import { apiClient } from '../../api/httpClient'
import type { PaginatedList } from '../../types/pagination'
import type {
  Personnel,
  PersonnelFormValues,
  PersonnelListItem,
  PersonnelQuery,
  UpdatePersonnelPayload,
} from './personnelTypes'

export async function searchPersonnel(query: PersonnelQuery) {
  const response = await apiClient.get<PaginatedList<PersonnelListItem>>('/api/personnel', {
    params: query,
  })

  return response.data
}

export async function createPersonnel(values: PersonnelFormValues) {
  const response = await apiClient.post<Personnel>('/api/personnel', toApiPayload(values))

  return response.data
}

export async function updatePersonnel(id: string, values: UpdatePersonnelPayload) {
  const response = await apiClient.put<Personnel>(`/api/personnel/${id}`, toApiPayload(values))

  return response.data
}

export async function updatePersonnelStatus(id: string, isActive: boolean) {
  const response = await apiClient.patch<Personnel>(`/api/personnel/${id}/status`, { isActive })

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
