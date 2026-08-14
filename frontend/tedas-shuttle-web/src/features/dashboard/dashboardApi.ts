import { apiClient } from '../../api/httpClient'
import type { DashboardSummary } from './dashboardTypes'

export async function getDashboardSummary() {
  const response = await apiClient.get<DashboardSummary>('/api/dashboard/summary')

  return response.data
}
