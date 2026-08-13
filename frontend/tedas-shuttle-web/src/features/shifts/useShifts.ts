import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createShift, listShifts, updateShift, updateShiftStatus } from './shiftApi'
import type { ShiftFormValues } from './shiftTypes'

export function useShifts(shuttleId: string | undefined) {
  return useQuery({
    queryKey: ['shifts', shuttleId],
    queryFn: () => listShifts(shuttleId ?? ''),
    enabled: !!shuttleId,
  })
}

export function useCreateShift(shuttleId: string | undefined) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: ShiftFormValues) => createShift(shuttleId ?? '', values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['shifts', shuttleId] }),
  })
}

export function useUpdateShift(shuttleId: string | undefined) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, values }: { id: string; values: ShiftFormValues }) =>
      updateShift(id, values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['shifts', shuttleId] }),
  })
}

export function useUpdateShiftStatus(shuttleId: string | undefined) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      updateShiftStatus(id, isActive),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['shifts', shuttleId] }),
  })
}

