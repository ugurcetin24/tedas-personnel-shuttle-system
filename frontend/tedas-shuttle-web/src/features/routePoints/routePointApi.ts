import { apiClient } from '../../api/httpClient'
import type { RoutePoint, RoutePointFormValues, RoutePointListItem } from './routePointTypes'

export async function listRoutePoints(shiftId: string) {
  const response = await apiClient.get<RoutePointListItem[]>(`/api/shifts/${shiftId}/route-points`)

  return response.data
}

export async function createRoutePoint(shiftId: string, values: RoutePointFormValues) {
  const response = await apiClient.post<RoutePoint>(
    `/api/shifts/${shiftId}/route-points`,
    toApiPayload(values),
  )

  return response.data
}

export async function updateRoutePoint(id: string, values: RoutePointFormValues) {
  const response = await apiClient.put<RoutePoint>(`/api/route-points/${id}`, toApiPayload(values))

  return response.data
}

export async function updateRoutePointStatus(id: string, isActive: boolean) {
  const response = await apiClient.patch<RoutePoint>(`/api/route-points/${id}/status`, { isActive })

  return response.data
}

export async function reorderRoutePoints(shiftId: string, routePointIds: string[]) {
  const response = await apiClient.patch<RoutePointListItem[]>(
    `/api/shifts/${shiftId}/route-points/order`,
    { routePointIds },
  )

  return response.data
}

function toApiPayload(values: RoutePointFormValues) {
  return {
    ...values,
    address: values.address.trim() === '' ? null : values.address,
  }
}

