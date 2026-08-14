import { apiClient } from '../../api/httpClient'
import type { CalculatedRoute, SavedRoute } from './routingTypes'

export async function calculateRoute(shiftId: string) {
  const response = await apiClient.post<CalculatedRoute>(`/api/shifts/${shiftId}/routes/calculate`)

  return response.data
}

export async function listSavedRoutes(shiftId: string) {
  const response = await apiClient.get<SavedRoute[]>(`/api/shifts/${shiftId}/routes`)

  return response.data
}

export async function saveCalculatedRoute(shiftId: string, name: string) {
  const response = await apiClient.post<SavedRoute>(`/api/shifts/${shiftId}/routes`, { name })

  return response.data
}
