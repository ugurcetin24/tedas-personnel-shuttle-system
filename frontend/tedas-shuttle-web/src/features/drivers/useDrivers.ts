import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createDriver,
  searchDrivers,
  updateDriver,
  updateDriverShiftAssignment,
  updateDriverStatus,
} from './driverApi'
import type { DriverFormValues, DriverQuery, UpdateDriverPayload } from './driverTypes'

export function useDrivers(query: DriverQuery) {
  return useQuery({
    queryKey: ['drivers', query],
    queryFn: () => searchDrivers(query),
  })
}

export function useCreateDriver() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: DriverFormValues) => createDriver(values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['drivers'] }),
  })
}

export function useUpdateDriver() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, values }: { id: string; values: UpdateDriverPayload }) =>
      updateDriver(id, values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['drivers'] }),
  })
}

export function useUpdateDriverStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      updateDriverStatus(id, isActive),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['drivers'] }),
  })
}

export function useUpdateDriverShiftAssignment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, shuttleShiftId }: { id: string; shuttleShiftId: string | null }) =>
      updateDriverShiftAssignment(id, shuttleShiftId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['drivers'] }),
  })
}

