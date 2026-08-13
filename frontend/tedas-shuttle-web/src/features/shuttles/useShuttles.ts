import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createShuttle,
  searchShuttles,
  updateShuttle,
  updateShuttleStatus,
} from './shuttleApi'
import type { ShuttleFormValues, ShuttleQuery, UpdateShuttlePayload } from './shuttleTypes'

export function useShuttles(query: ShuttleQuery) {
  return useQuery({
    queryKey: ['shuttles', query],
    queryFn: () => searchShuttles(query),
  })
}

export function useCreateShuttle() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: ShuttleFormValues) => createShuttle(values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['shuttles'] }),
  })
}

export function useUpdateShuttle() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, values }: { id: string; values: UpdateShuttlePayload }) =>
      updateShuttle(id, values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['shuttles'] }),
  })
}

export function useUpdateShuttleStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      updateShuttleStatus(id, isActive),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['shuttles'] }),
  })
}
