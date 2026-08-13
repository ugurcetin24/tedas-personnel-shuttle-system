import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createRoutePoint,
  listRoutePoints,
  reorderRoutePoints,
  updateRoutePoint,
  updateRoutePointStatus,
} from './routePointApi'
import type { RoutePointFormValues } from './routePointTypes'

export function useRoutePoints(shiftId: string | undefined) {
  return useQuery({
    queryKey: ['route-points', shiftId],
    queryFn: () => listRoutePoints(shiftId ?? ''),
    enabled: !!shiftId,
  })
}

export function useCreateRoutePoint(shiftId: string | undefined) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: RoutePointFormValues) => createRoutePoint(shiftId ?? '', values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['route-points', shiftId] }),
  })
}

export function useUpdateRoutePoint(shiftId: string | undefined) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, values }: { id: string; values: RoutePointFormValues }) =>
      updateRoutePoint(id, values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['route-points', shiftId] }),
  })
}

export function useUpdateRoutePointStatus(shiftId: string | undefined) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      updateRoutePointStatus(id, isActive),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['route-points', shiftId] }),
  })
}

export function useReorderRoutePoints(shiftId: string | undefined) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (routePointIds: string[]) => reorderRoutePoints(shiftId ?? '', routePointIds),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['route-points', shiftId] }),
  })
}

