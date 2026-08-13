import { apiClient } from '../../api/httpClient'
import type { PaginatedList } from '../../types/pagination'
import type {
  Driver,
  DriverFormValues,
  DriverListItem,
  DriverQuery,
  UpdateDriverPayload,
} from './driverTypes'

export async function searchDrivers(query: DriverQuery) {
  const response = await apiClient.get<PaginatedList<DriverListItem>>('/api/drivers', {
    params: query,
  })

  return response.data
}

export async function createDriver(values: DriverFormValues) {
  const response = await apiClient.post<Driver>('/api/drivers', toApiPayload(values))

  return response.data
}

export async function updateDriver(id: string, values: UpdateDriverPayload) {
  const response = await apiClient.put<Driver>(`/api/drivers/${id}`, toApiPayload(values))

  return response.data
}

export async function updateDriverStatus(id: string, isActive: boolean) {
  const response = await apiClient.patch<Driver>(`/api/drivers/${id}/status`, { isActive })

  return response.data
}

export async function updateDriverShiftAssignment(id: string, shuttleShiftId: string | null) {
  const response = await apiClient.patch<Driver>(`/api/drivers/${id}/shift-assignment`, {
    shuttleShiftId,
  })

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

