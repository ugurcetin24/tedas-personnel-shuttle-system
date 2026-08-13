import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createPersonnel,
  searchPersonnel,
  updatePersonnel,
  updatePersonnelStatus,
} from './personnelApi'
import type { PersonnelFormValues, PersonnelQuery, UpdatePersonnelPayload } from './personnelTypes'

export function usePersonnel(query: PersonnelQuery) {
  return useQuery({
    queryKey: ['personnel', query],
    queryFn: () => searchPersonnel(query),
  })
}

export function useCreatePersonnel() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: PersonnelFormValues) => createPersonnel(values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['personnel'] }),
  })
}

export function useUpdatePersonnel() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, values }: { id: string; values: UpdatePersonnelPayload }) =>
      updatePersonnel(id, values),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['personnel'] }),
  })
}

export function useUpdatePersonnelStatus() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      updatePersonnelStatus(id, isActive),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['personnel'] }),
  })
}
