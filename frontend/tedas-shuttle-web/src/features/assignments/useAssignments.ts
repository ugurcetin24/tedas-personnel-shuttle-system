import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { createAssignment, deactivateAssignment, searchAssignments } from './assignmentApi'
import type { AssignmentFormValues, AssignmentQuery } from './assignmentTypes'

export function useAssignments(query: AssignmentQuery) {
  return useQuery({
    queryKey: ['assignments', query],
    queryFn: () => searchAssignments(query),
  })
}

export function useCreateAssignment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (values: AssignmentFormValues) => createAssignment(values),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assignments'] })
      queryClient.invalidateQueries({ queryKey: ['shifts'] })
    },
  })
}

export function useDeactivateAssignment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (id: string) => deactivateAssignment(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['assignments'] })
      queryClient.invalidateQueries({ queryKey: ['shifts'] })
    },
  })
}

