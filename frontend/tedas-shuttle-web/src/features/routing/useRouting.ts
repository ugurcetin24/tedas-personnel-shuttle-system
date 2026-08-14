import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { calculateRoute, listSavedRoutes, saveCalculatedRoute } from './routingApi'

export function useSavedRoutes(shiftId: string | undefined) {
  return useQuery({
    queryKey: ['saved-routes', shiftId],
    queryFn: () => listSavedRoutes(shiftId ?? ''),
    enabled: !!shiftId,
  })
}

export function useCalculateRoute(shiftId: string | undefined) {
  return useMutation({
    mutationFn: () => calculateRoute(shiftId ?? ''),
  })
}

export function useSaveCalculatedRoute(shiftId: string | undefined) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (name: string) => saveCalculatedRoute(shiftId ?? '', name),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['saved-routes', shiftId] }),
  })
}
