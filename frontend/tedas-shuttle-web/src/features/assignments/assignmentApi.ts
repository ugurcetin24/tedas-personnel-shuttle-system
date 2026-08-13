import { apiClient } from '../../api/httpClient'
import type { PaginatedList } from '../../types/pagination'
import type {
  Assignment,
  AssignmentFormValues,
  AssignmentListItem,
  AssignmentQuery,
} from './assignmentTypes'

export async function searchAssignments(query: AssignmentQuery) {
  const response = await apiClient.get<PaginatedList<AssignmentListItem>>('/api/assignments', {
    params: query,
  })

  return response.data
}

export async function createAssignment(values: AssignmentFormValues) {
  const response = await apiClient.post<Assignment>('/api/assignments', {
    personnelId: values.personnelId,
    shuttleShiftId: values.shuttleShiftId,
    boardingRoutePointId: null,
  })

  return response.data
}

export async function deactivateAssignment(id: string) {
  const response = await apiClient.delete<Assignment>(`/api/assignments/${id}`)

  return response.data
}

