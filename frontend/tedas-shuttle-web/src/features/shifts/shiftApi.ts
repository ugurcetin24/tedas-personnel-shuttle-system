import { apiClient } from '../../api/httpClient'
import type { Shift, ShiftFormValues, ShiftListItem } from './shiftTypes'

export async function listShifts(shuttleId: string) {
  const response = await apiClient.get<ShiftListItem[]>(`/api/shuttles/${shuttleId}/shifts`)

  return response.data
}

export async function listAllShifts(isActive?: boolean) {
  const response = await apiClient.get<ShiftListItem[]>('/api/shifts', {
    params: { isActive },
  })

  return response.data
}

export async function createShift(shuttleId: string, values: ShiftFormValues) {
  const response = await apiClient.post<Shift>(
    `/api/shuttles/${shuttleId}/shifts`,
    toApiPayload(values),
  )

  return response.data
}

export async function updateShift(id: string, values: ShiftFormValues) {
  const response = await apiClient.put<Shift>(`/api/shifts/${id}`, toApiPayload(values))

  return response.data
}

export async function updateShiftStatus(id: string, isActive: boolean) {
  const response = await apiClient.patch<Shift>(`/api/shifts/${id}/status`, { isActive })

  return response.data
}

function toApiPayload(values: ShiftFormValues) {
  return {
    ...values,
    startTime: normalizeTime(values.startTime),
    endTime: normalizeTime(values.endTime),
  }
}

function normalizeTime(value: string) {
  return value.length === 5 ? `${value}:00` : value
}
